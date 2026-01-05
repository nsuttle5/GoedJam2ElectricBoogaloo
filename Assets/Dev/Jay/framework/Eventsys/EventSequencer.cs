// Assets/Scripts/Events/EventSequencer.cs
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EventSequencer : MonoBehaviour
{
    [Header("Playlist")]
    [Tooltip("Sequences will play in order. Null entries are skipped.")]
    [SerializeField] private List<EventSequenceAsset> playlist = new();

    [Tooltip("If true, loops back to playlist index 0 after the last sequence.")]
    [SerializeField] private bool loopPlaylist = false;

    [Tooltip("If true, automatically starts playing on enable.")]
    [SerializeField] private bool playOnEnable = true;

    [Tooltip("If true, uses unscaled time (ignores Time.timeScale).")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Start")]
    [SerializeField, Min(0)] private int startPlaylistIndex = 0;
    [SerializeField, Min(0f)] private float startTimeSeconds = 0f;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = false;

    // runtime
    private readonly List<EventSequenceAsset.TimedEvent> _sorted = new();
    private int _currentPlaylistIndex = -1;
    private int _nextEventIndex = 0;
    private float _time = 0f;
    private bool _playing = false;

    public int CurrentPlaylistIndex => _currentPlaylistIndex;
    public float TimeSeconds => _time;
    public bool IsPlaying => _playing;

    private void OnEnable()
    {
        if (playOnEnable)
            PlayFrom(startPlaylistIndex, startTimeSeconds);
    }

    private void Update()
    {
        if (!_playing) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        _time += dt;

        // Fire any due events for current sequence
        FireDueEvents();

        // If sequence ended, advance
        if (IsCurrentSequenceFinished())
            AdvanceToNextSequence();
    }

    // ---------- Public API ----------

    public void PlayFrom(int playlistIndex, float timeSeconds = 0f)
    {
        _playing = true;
        SetCurrentSequence(playlistIndex, timeSeconds);
    }

    public void Stop() => _playing = false;

    public void Pause() => _playing = false;
    public void Resume() => _playing = true;

    public void SetPlaylist(List<EventSequenceAsset> newPlaylist, bool restart = true)
    {
        playlist = newPlaylist ?? new List<EventSequenceAsset>();

        if (restart)
            PlayFrom(startPlaylistIndex, startTimeSeconds);
        else
            RebuildCacheForCurrent(); // keep current index if possible
    }

    public void JumpToNextSequence()
    {
        AdvanceToNextSequence(forceEvenIfNotFinished: true);
    }

    public void JumpToPreviousSequence()
    {
        int prev = FindPrevValidIndex(_currentPlaylistIndex);
        if (prev < 0)
        {
            if (verboseLogs) Debug.Log("[EventSequencer] No previous sequence.", this);
            return;
        }

        SetCurrentSequence(prev, 0f);
    }

    public void RestartCurrentSequence(float timeSeconds = 0f)
    {
        if (_currentPlaylistIndex < 0) return;
        SetCurrentSequence(_currentPlaylistIndex, timeSeconds);
    }

    // ---------- Internals ----------

    private void SetCurrentSequence(int requestedIndex, float startTime)
    {
        int idx = FindNextValidIndex(requestedIndex - 1); // so next search starts at requestedIndex
        if (idx < 0)
        {
            _playing = false;
            _currentPlaylistIndex = -1;
            _sorted.Clear();
            if (verboseLogs) Debug.Log("[EventSequencer] Playlist empty or only null entries. Stopping.", this);
            return;
        }

        _currentPlaylistIndex = idx;
        _time = Mathf.Max(0f, startTime);

        RebuildCacheForCurrent();

        // If starting at/after some events, skip to the first event not yet passed.
        _nextEventIndex = FindFirstEventIndexAtOrAfter(_time);

        if (verboseLogs)
            Debug.Log($"[EventSequencer] Now playing playlist[{_currentPlaylistIndex}] '{playlist[_currentPlaylistIndex].name}' from t={_time:0.000}", this);

        // Fire events that are due immediately (e.g., startTime == 0 and time == 0)
        FireDueEvents();
    }

    private void RebuildCacheForCurrent()
    {
        _sorted.Clear();

        if (_currentPlaylistIndex < 0 || _currentPlaylistIndex >= playlist.Count) return;

        var seq = playlist[_currentPlaylistIndex];
        if (seq == null || seq.events == null) return;

        _sorted.AddRange(seq.events);
        _sorted.Sort((a, b) => a.time.CompareTo(b.time));
    }

    private void FireDueEvents()
    {
        while (_nextEventIndex < _sorted.Count && _sorted[_nextEventIndex].time <= _time)
        {
            var ev = _sorted[_nextEventIndex];
            _nextEventIndex++;

            if (verboseLogs)
                Debug.Log($"[EventSequencer] t={_time:0.000} fired '{ev.eventKey}' (scheduled {ev.time:0.000})", this);

            GameEventBus.Broadcast(ev.eventKey, _time, this);
        }
    }

    private bool IsCurrentSequenceFinished()
    {
        // Finished means: we have fired all events for this sequence.
        return _nextEventIndex >= _sorted.Count;
    }

    private void AdvanceToNextSequence(bool forceEvenIfNotFinished = false)
    {
        if (!forceEvenIfNotFinished && !IsCurrentSequenceFinished())
            return;

        int next = FindNextValidIndex(_currentPlaylistIndex);
        if (next < 0)
        {
            if (loopPlaylist)
            {
                next = FindNextValidIndex(-1);
                if (next >= 0)
                {
                    if (verboseLogs) Debug.Log("[EventSequencer] Looping playlist to start.", this);
                    SetCurrentSequence(next, 0f);
                    return;
                }
            }

            if (verboseLogs) Debug.Log("[EventSequencer] Reached end of playlist. Stopping.", this);
            _playing = false;
            return;
        }

        SetCurrentSequence(next, 0f);
    }

    private int FindNextValidIndex(int startIndexInclusive)
    {
        if (playlist == null || playlist.Count == 0) return -1;

        for (int i = startIndexInclusive + 1; i < playlist.Count; i++)
            if (playlist[i] != null)
                return i;

        return -1;
    }

    private int FindPrevValidIndex(int startIndexExclusive)
    {
        if (playlist == null || playlist.Count == 0) return -1;

        int start = Mathf.Clamp(startIndexExclusive - 1, playlist.Count - 1, playlist.Count - 1);
        for (int i = start; i >= 0; i--)
            if (playlist[i] != null)
                return i;

        return -1;
    }

    private int FindFirstEventIndexAtOrAfter(float t)
    {
        for (int i = 0; i < _sorted.Count; i++)
            if (_sorted[i].time >= t)
                return i;
        return _sorted.Count;
    }
}
