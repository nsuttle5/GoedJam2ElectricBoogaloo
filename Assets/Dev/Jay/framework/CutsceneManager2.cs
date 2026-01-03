using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class CutsceneManager2 : MonoBehaviour
{
    public static CutsceneManager2 Instance { get; private set; }

    [Header("Startup Loading")]
    [Tooltip("Scenes to load additively at startup (after Core).")]
    [SerializeField] private List<string> additiveScenesToLoad = new();

    [Tooltip("If true, plays automatically after all additive scenes finish loading.")]
    [SerializeField] private bool playOnStart = true;

    [Header("Cutscene Shots (in order)")]
    [SerializeField] private List<Shot> shots = new();

    [Header("Cinemachine Priorities")]
    [SerializeField] private int basePriority = 0;

    [Tooltip("Priority given to the active shot camera.")]
    [SerializeField] private int activePriority = 20;

    [Header("End Behavior")]
    [Tooltip("If set, we preload this scene during the last shot and activate it when the cutscene ends.")]
    [SerializeField] private string nextSceneSingleLoad = "";

    [Tooltip("If true, we start loading nextSceneSingleLoad near the end (during the last shot).")]
    [SerializeField] private bool preloadNextSceneDuringLastShot = true;

    [Tooltip("Seconds before the cutscene ends to start preloading next scene (clamped).")]
    [SerializeField] private float preloadLeadSeconds = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = false;

    [Serializable]
    public struct Shot
    {
        public string vcamId;
        [Min(0f)] public float duration;
        [Tooltip("Optional per-shot override. If <= 0 uses activePriority.")]
        public int priorityOverride;
    }

    private readonly Dictionary<string, CinemachineCamera> _vcamMap = new();
    private Coroutine _playRoutine;
    private AsyncOperation _nextSceneLoadOp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Kick off startup flow
        StartCoroutine(StartupFlow());
    }

    private IEnumerator StartupFlow()
    {
        // 1) Load additive scenes
        yield return LoadAdditiveScenes(additiveScenesToLoad);

        // 2) Build vcam registry
        RebuildVcamRegistry();

        // 3) Play
        if (playOnStart)
            PlayCutscene();
    }

    public void PlayCutscene()
    {
        if (_playRoutine != null) StopCoroutine(_playRoutine);
        _playRoutine = StartCoroutine(PlayRoutine());
    }

    public void StopCutscene()
    {
        if (_playRoutine != null) StopCoroutine(_playRoutine);
        _playRoutine = null;
        SetAllVcamsToBasePriority();
    }

    public void RebuildVcamRegistry()
    {
        _vcamMap.Clear();

        // Find all VcamId in loaded scenes (including inactive objects)
        var ids = FindObjectsByType<VCamID>(FindObjectsSortMode.InstanceID);

        foreach (var vid in ids)
        {
            if (vid == null) continue;
            if (string.IsNullOrWhiteSpace(vid.id)) continue;

            var cam = vid.GetCamera();
            if (cam == null)
            {
                Debug.LogWarning($"[CutsceneManager] VcamId '{vid.id}' has no CinemachineCamera on '{vid.gameObject.name}'.");
                continue;
            }

            if (_vcamMap.ContainsKey(vid.id))
            {
                Debug.LogError($"[CutsceneManager] Duplicate vcam id '{vid.id}'. IDs must be unique. Offender: '{vid.gameObject.name}'.");
                continue;
            }

            _vcamMap.Add(vid.id, cam);
        }

        SetAllVcamsToBasePriority();

        if (verboseLogs)
            Debug.Log($"[CutsceneManager] Registered {_vcamMap.Count} vcams: {string.Join(", ", _vcamMap.Keys)}");
    }

    private IEnumerator PlayRoutine()
    {
        if (shots == null || shots.Count == 0)
        {
            Debug.LogWarning("[CutsceneManager] No shots configured.");
            yield break;
        }

        // Rebuild in case scenes changed
        RebuildVcamRegistry();

        // Optionally begin preloading the next scene near the end
        int lastIndex = shots.Count - 1;

        for (int i = 0; i < shots.Count; i++)
        {
            var shot = shots[i];

            if (!_vcamMap.TryGetValue(shot.vcamId, out var vcam) || vcam == null)
            {
                Debug.LogError($"[CutsceneManager] Missing vcam id '{shot.vcamId}' for shot {i}. Skipping timing only.");
                yield return new WaitForSeconds(shot.duration);
                continue;
            }

            // If we're on the last shot, optionally start preloading the next scene partway through it
            if (i == lastIndex && preloadNextSceneDuringLastShot && !string.IsNullOrWhiteSpace(nextSceneSingleLoad))
            {
                float lead = Mathf.Clamp(preloadLeadSeconds, 0f, shot.duration);
                float firstPart = Mathf.Max(0f, shot.duration - lead);

                ActivateVcam(vcam, shot.priorityOverride);

                if (firstPart > 0f)
                    yield return new WaitForSeconds(firstPart);

                BeginPreloadNextSceneSingle();
                if (lead > 0f)
                    yield return new WaitForSeconds(lead);
            }
            else
            {
                ActivateVcam(vcam, shot.priorityOverride);
                yield return new WaitForSeconds(shot.duration);
            }
        }

        // Cutscene ends
        SetAllVcamsToBasePriority();

        if (!string.IsNullOrWhiteSpace(nextSceneSingleLoad))
        {
            // Ensure load op exists
            if (_nextSceneLoadOp == null)
                BeginPreloadNextSceneSingle();

            // If we never started preload, load normally
            if (_nextSceneLoadOp != null)
            {
                _nextSceneLoadOp.allowSceneActivation = true;
                while (!_nextSceneLoadOp.isDone)
                    yield return null;
            }
            else
            {
                SceneManager.LoadScene(nextSceneSingleLoad, LoadSceneMode.Single);
            }
        }

        _playRoutine = null;
    }

    private void ActivateVcam(CinemachineCamera active, int priorityOverride)
    {
        SetAllVcamsToBasePriority();

        int p = (priorityOverride > 0) ? priorityOverride : activePriority;
        active.Priority = p;

        if (verboseLogs)
            Debug.Log($"[CutsceneManager] Active shot: {active.gameObject.name} (priority {p})");
    }

    private void SetAllVcamsToBasePriority()
    {
        foreach (var kv in _vcamMap)
        {
            if (kv.Value != null)
                kv.Value.Priority = basePriority;
        }
    }

    private IEnumerator LoadAdditiveScenes(List<string> sceneNames)
    {
        if (sceneNames == null || sceneNames.Count == 0)
            yield break;

        // Load each scene additively (skip already-loaded scenes)
        foreach (var sceneName in sceneNames)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) continue;

            if (IsSceneLoaded(sceneName))
                continue;

            if (verboseLogs) Debug.Log($"[CutsceneManager] Loading additive scene '{sceneName}'...");
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!op.isDone)
                yield return null;
        }
    }

    private static bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name == sceneName)
                return true;
        }
        return false;
    }

    private void BeginPreloadNextSceneSingle()
    {
        if (_nextSceneLoadOp != null) return;

        if (string.IsNullOrWhiteSpace(nextSceneSingleLoad))
            return;

        if (verboseLogs) Debug.Log($"[CutsceneManager] Preloading next scene '{nextSceneSingleLoad}' (Single)...");
        _nextSceneLoadOp = SceneManager.LoadSceneAsync(nextSceneSingleLoad, LoadSceneMode.Single);
        _nextSceneLoadOp.allowSceneActivation = false;
    }
}
