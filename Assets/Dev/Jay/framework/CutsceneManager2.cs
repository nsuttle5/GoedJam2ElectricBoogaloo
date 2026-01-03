using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class CutsceneManager2 : MonoBehaviour
{
    public static CutsceneManager2 Instance { get; private set; }

    [Header("Defaults")]
    [SerializeField] private int basePriority = 0;
    [SerializeField] private int activePriority = 20;

    [Tooltip("If empty, we'll try to find one in the scene.")]
    [SerializeField] private CinemachineBrain brain;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = false;

    private readonly Dictionary<string, CinemachineCamera> _vcams = new();
    private Coroutine _routine;
    private AsyncOperation _nextSceneOp;

    // Store/restore brain blend
    private CinemachineBlendDefinition _savedBlend;
    private bool _savedBlendValid;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void EnsureBrain()
    {
        if (brain != null) return;
        brain = FindFirstObjectByType<CinemachineBrain>();
        if (brain == null)
            Debug.LogError("[CutsceneManager] No CinemachineBrain found. Add CinemachineBrain to your Main Camera.");
    }

    public void Play(CutsceneAsset cutscene)
    {
        if (cutscene == null) { Debug.LogError("[CutsceneManager] CutsceneAsset is null."); return; }

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(PlayRoutine(cutscene));
    }

    public void Stop()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;

        SetAllBasePriority();
        RestoreBrainBlend();
    }

    private IEnumerator PlayRoutine(CutsceneAsset cutscene)
    {
        EnsureBrain();
        if (brain == null) yield break;

        // Only created if any fade is used
        ScreenFader fader = null;

        // 1) Load additive scenes
        yield return LoadAdditiveScenes(cutscene.additiveScenesToLoad);

        // 2) Build registry
        RebuildRegistry();

        if (cutscene.shots == null || cutscene.shots.Count == 0)
        {
            Debug.LogWarning("[CutsceneManager] Cutscene has no shots.");
            yield break;
        }

        // 3) Play shots
        int last = cutscene.shots.Count - 1;

        for (int i = 0; i < cutscene.shots.Count; i++)
        {
            var shot = cutscene.shots[i];

            // Blend override for transition INTO this shot
            if (shot.overrideBlend)
                ApplyBrainBlend(shot.blendStyle, shot.blendTime);

            if (!_vcams.TryGetValue(shot.vCamID, out var vcam) || vcam == null)
            {
                Debug.LogError($"[CutsceneManager] Missing vcam id '{shot.vCamID}' (shot {i}).");
                yield return new WaitForSeconds(shot.duration);
                continue;
            }

            // Fade-in means fade TO transparent at the start of this shot
            if (shot.fadeIn)
            {
                fader ??= ScreenFader.EnsureExists();
                var target = shot.fadeColor; target.a = 0f;
                fader.FadeTo(target, shot.fadeInTime, false); // keep your fader signature, always scaled time
            }

            Activate(vcam);

            // Last-shot preload logic
            if (i == last && cutscene.preloadNextSceneDuringLastShot && !string.IsNullOrWhiteSpace(cutscene.nextSceneSingleLoad))
            {
                float lead = Mathf.Clamp(cutscene.preloadLeadSeconds, 0f, shot.duration);
                float firstPart = Mathf.Max(0f, shot.duration - lead);

                if (firstPart > 0f) yield return new WaitForSeconds(firstPart);

                BeginPreloadNextSingle(cutscene.nextSceneSingleLoad);

                if (lead > 0f) yield return new WaitForSeconds(lead);
            }
            else
            {
                // Fade-out near end of shot (fade TO opaque)
                if (shot.fadeOut && shot.fadeOutTime > 0f && shot.fadeOutTime < shot.duration)
                {
                    float before = shot.duration - shot.fadeOutTime;
                    yield return new WaitForSeconds(before);

                    fader ??= ScreenFader.EnsureExists();
                    var target = shot.fadeColor; target.a = 1f;
                    fader.FadeTo(target, shot.fadeOutTime, false);

                    yield return new WaitForSeconds(shot.fadeOutTime);
                }
                else
                {
                    yield return new WaitForSeconds(shot.duration);

                    // Instant fade-out at end if fadeOutTime <= 0
                    if (shot.fadeOut && shot.fadeOutTime <= 0f)
                    {
                        fader ??= ScreenFader.EnsureExists();
                        var target = shot.fadeColor; target.a = 1f;
                        fader.FadeTo(target, 0f, false);
                    }
                }
            }
        }

        // End cleanup
        SetAllBasePriority();
        RestoreBrainBlend();

        // 4) End scene load
        if (!string.IsNullOrWhiteSpace(cutscene.nextSceneSingleLoad))
        {
            if (_nextSceneOp == null)
                BeginPreloadNextSingle(cutscene.nextSceneSingleLoad);

            if (_nextSceneOp != null)
            {
                _nextSceneOp.allowSceneActivation = true;
                while (!_nextSceneOp.isDone) yield return null;
            }
            else
            {
                SceneManager.LoadScene(cutscene.nextSceneSingleLoad, LoadSceneMode.Single);
            }
        }

        _routine = null;
    }

    private void RebuildRegistry()
    {
        _vcams.Clear();

        // IMPORTANT: make sure this matches your component name.
        // If your component is VcamId, change VCamID -> VcamId here.
        var ids = FindObjectsByType<VCamID>(FindObjectsSortMode.InstanceID);

        foreach (var vid in ids)
        {
            if (vid == null) continue;
            if (string.IsNullOrWhiteSpace(vid.id)) continue;

            var cam = vid.GetCamera();
            if (cam == null) continue;

            if (_vcams.ContainsKey(vid.id))
            {
                Debug.LogError($"[CutsceneManager] Duplicate vcam id '{vid.id}'. IDs must be unique.");
                continue;
            }

            _vcams.Add(vid.id, cam);
        }

        SetAllBasePriority();

        if (verboseLogs)
            Debug.Log($"[CutsceneManager] Registered {_vcams.Count} vcams: {string.Join(", ", _vcams.Keys)}");
    }

    private void SetAllBasePriority()
    {
        foreach (var kv in _vcams)
            if (kv.Value != null) kv.Value.Priority = basePriority;
    }

    private void Activate(CinemachineCamera cam)
    {
        SetAllBasePriority();
        cam.Priority = activePriority;

        if (verboseLogs)
            Debug.Log($"[CutsceneManager] Active: {cam.name} (priority {activePriority})");
    }

    private IEnumerator LoadAdditiveScenes(List<string> sceneNames)
    {
        if (sceneNames == null) yield break;

        foreach (var sceneName in sceneNames)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) continue;
            if (IsSceneLoaded(sceneName)) continue;

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!op.isDone) yield return null;
        }
    }

    private static bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name == sceneName) return true;
        }
        return false;
    }

    private void BeginPreloadNextSingle(string sceneName)
    {
        if (_nextSceneOp != null) return;
        _nextSceneOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        _nextSceneOp.allowSceneActivation = false;
    }

    private void ApplyBrainBlend(CutsceneAsset.BlendStyle style, float time)
    {
        if (!_savedBlendValid)
        {
            _savedBlend = brain.DefaultBlend;
            _savedBlendValid = true;
        }

        if (style == CutsceneAsset.BlendStyle.Cut)
        {
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            return;
        }

        var cmStyle = style switch
        {
            CutsceneAsset.BlendStyle.EaseInOut => CinemachineBlendDefinition.Styles.EaseInOut,
            CutsceneAsset.BlendStyle.EaseIn => CinemachineBlendDefinition.Styles.EaseIn,
            CutsceneAsset.BlendStyle.EaseOut => CinemachineBlendDefinition.Styles.EaseOut,
            CutsceneAsset.BlendStyle.Linear => CinemachineBlendDefinition.Styles.Linear,
            _ => CinemachineBlendDefinition.Styles.EaseInOut
        };

        brain.DefaultBlend = new CinemachineBlendDefinition(cmStyle, Mathf.Max(0f, time));
    }

    private void RestoreBrainBlend()
    {
        if (brain == null || !_savedBlendValid) return;
        brain.DefaultBlend = _savedBlend;
        _savedBlendValid = false;
    }
}
