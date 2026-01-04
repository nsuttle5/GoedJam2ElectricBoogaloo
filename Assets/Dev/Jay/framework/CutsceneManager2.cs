using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class CutsceneManager2 : MonoBehaviour
{
    public static CutsceneManager2 Instance { get; private set; }

    [Header("Debug")]
    public int shotNumber = 0;
    [SerializeField] private bool verboseLogs = false;

    [Header("Playlist")]
    [Tooltip("Cutscenes played in order.")]
    [SerializeField] private List<CutsceneAsset> playlist = new();




    [Tooltip("If true, plays the playlist on Start.")]
    [SerializeField] private bool playPlaylistOnStart = true;

    [Tooltip("Index in the playlist to start from.")]
    [SerializeField, Min(0)] private int startIndex = 0;

    [Header("Defaults")]
    [SerializeField] private int basePriority = 0;
    [SerializeField] private int activePriority = 20;

    [Tooltip("If empty, we'll try to find one in the scene.")]
    [SerializeField] private CinemachineBrain brain;

    private readonly Dictionary<string, CinemachineCamera> _vcams = new();
    private Coroutine _routine;

    // Store/restore brain blend
    private CinemachineBlendDefinition _savedBlend;
    private bool _savedBlendValid;

    // Preload op for *current* cutscene's next-scene (if any)
    private AsyncOperation _nextSceneOp;

    private int _currentPlaylistIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }




    private void Start()
    {
        if (playPlaylistOnStart)
            PlayPlaylist(startIndex);
    }

    private void EnsureBrain()
    {
        if (brain != null) return;
        brain = FindFirstObjectByType<CinemachineBrain>();
        if (brain == null)
            Debug.LogError("[CutsceneManager] No CinemachineBrain found. Add CinemachineBrain to your Main Camera.");
    }




    // --- Public API ---

    public void PlayPlaylist(int fromIndex = 0)
    {
        EnsureBrain();
        if (brain == null) return;

        if (playlist == null || playlist.Count == 0)
        {
            Debug.LogWarning("[CutsceneManager] Playlist is empty.");
            return;
        }

        fromIndex = Mathf.Clamp(fromIndex, 0, playlist.Count - 1);

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(PlayPlaylistRoutine(fromIndex));



    }

    public void PlayIndex(int index) => PlayPlaylist(index);

    public void PlayNext()
    {
        if (playlist == null || playlist.Count == 0) return;
        int next = Mathf.Clamp(_currentPlaylistIndex + 1, 0, playlist.Count - 1);
        PlayPlaylist(next);
    }

    public void Stop()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;

        _currentPlaylistIndex = -1;
        _nextSceneOp = null;

        SetAllBasePriority();
        RestoreBrainBlend();
    }

    // --- Routines ---

    private IEnumerator PlayPlaylistRoutine(int fromIndex)
    {



        //one ScreenFader shared across playlist
        ScreenFader fader = null;

        for (int i = fromIndex; i < playlist.Count; i++)
        {
            _currentPlaylistIndex = i;

            var cutscene = playlist[i];
            if (cutscene == null)
            {
                Debug.LogWarning($"[CutsceneManager] Playlist entry {i} is null. Skipping.");
                continue;
            }

            // Reset per-cutscene state
            shotNumber = 0;
            _nextSceneOp = null;




            if (verboseLogs)
                Debug.Log($"[CutsceneManager] Playing cutscene {i + 1}/{playlist.Count}: {cutscene.name}");

            // Play this cutscene; returns when done (or scene changes)
            yield return PlaySingleCutsceneRoutine(cutscene, fader);

            if (!string.IsNullOrWhiteSpace(cutscene.nextSceneSingleLoad))
            {
                if (verboseLogs)
                    Debug.Log("[CutsceneManager] Cutscene triggered Single scene load. Ending playlist.");
                break;
            }
        }

        _routine = null;
    }

    private IEnumerator PlaySingleCutsceneRoutine(CutsceneAsset cutscene, ScreenFader fader)
    {
        // 1) Load additive scenes for this cutscene
        yield return LoadAdditiveScenes(cutscene.additiveScenesToLoad);
        // 2) Build registry after scenes are loaded
        RebuildRegistry();

        if (cutscene.shots == null || cutscene.shots.Count == 0)
        {
            Debug.LogWarning($"[CutsceneManager] Cutscene '{cutscene.name}' has no shots.");
            yield break;
        }

        int last = cutscene.shots.Count - 1;

        for (int s = 0; s < cutscene.shots.Count; s++)
        {
            var shot = cutscene.shots[s];

            if (shot.overrideBlend)
                ApplyBrainBlend(shot.blendStyle, shot.blendTime);

            if (!_vcams.TryGetValue(shot.vCamID, out var vcam) || vcam == null)
            {
                Debug.LogError($"[CutsceneManager] Missing vcam id '{shot.vCamID}' (shot {s}) in cutscene '{cutscene.name}'.");
                yield return new WaitForSeconds(shot.duration);
                continue;
            }



            if (shot.fadeIn)
            {
                fader ??= ScreenFader.EnsureExists();
                var target = shot.fadeColor; target.a = 0f;
                fader.FadeTo(target, shot.fadeInTime, false);
            }

            Activate(vcam);

            // Last-shot preload logic
            if (s == last && cutscene.preloadNextSceneDuringLastShot && !string.IsNullOrWhiteSpace(cutscene.nextSceneSingleLoad))
            {
                float lead = Mathf.Clamp(cutscene.preloadLeadSeconds, 0f, shot.duration);
                float firstPart = Mathf.Max(0f, shot.duration - lead);

                if (firstPart > 0f) yield return new WaitForSeconds(firstPart);

                BeginPreloadNextSingle(cutscene.nextSceneSingleLoad);

                if (lead > 0f) yield return new WaitForSeconds(lead);
            }
            else
            {


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

                    if (shot.fadeOut && shot.fadeOutTime <= 0f)
                    {
                        fader ??= ScreenFader.EnsureExists();
                        var target = shot.fadeColor; target.a = 1f;
                        fader.FadeTo(target, 0f, false);
                    }
                }



            }
        }



        SetAllBasePriority();
        RestoreBrainBlend();

        // End scene load (Single) for this cutscene
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
    }

    // --- Registry / Cameras ---

    private void RebuildRegistry()
    {
        _vcams.Clear();

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
        shotNumber++;
        SetAllBasePriority();





        cam.Priority = activePriority;

        if (verboseLogs)
            Debug.Log($"[CutsceneManager] Active: {cam.name} (priority {activePriority})");
    }

    // --- Scene loading helpers ---

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



    // --- Blend override helpers ---

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
