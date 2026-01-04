// Assets/Scripts/Events/EventSequencer.cs
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EventSequencer : MonoBehaviour
{
    [Header("Sequence")]
    [SerializeField] private EventSequenceAsset sequence;

    [Header("Playback")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool useUnscaledTime = false;

    [Tooltip("Optional: start at this time when playing.")]
    [SerializeField, Min(0f)] private float startTime = 0f;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = false;

    private readonly List<EventSequenceAsset.TimedEvent> _sorted = new();
    private int _nextIndex;
    private float _time;
    private bool _playing;

    public float TimeSeconds => _time;
    public bool IsPlaying => _playing;

    private void OnEnable()
    {
        RebuildCache();

        if (playOnEnable)
            Play(startTime);
    }

    private void Update()
    {
        if (!_playing || _sorted.Count == 0) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        _time += dt;
        FireDueEvents();
    }

    public void SetSequence(EventSequenceAsset newSequence, bool restart = true)
    {
        sequence = newSequence;
        RebuildCache();
        if (restart) Play(startTime);
    }

    public void Play(float time = 0f)
    {
        _time = Mathf.Max(0f, time);
        _nextIndex = FindFirstEventIndexAtOrAfter(_time);
        _playing = true;

        // If we started exactly on an event time, we still want to fire it once when we pass it.
        FireDueEvents();
    }

    public void Stop() => _playing = false;

    public void Pause() => _playing = false;
    public void Resume() => _playing = true;

    public void ResetToStart()
    {
        _time = 0f;
        _nextIndex = 0;
    }

    private void RebuildCache()
    {
        _sorted.Clear();

        if (sequence == null || sequence.events == null) return;

        _sorted.AddRange(sequence.events);
        _sorted.Sort((a, b) => a.time.CompareTo(b.time));

        _nextIndex = 0;
        _time = 0f;
    }

    private void FireDueEvents()
    {
        while (_nextIndex < _sorted.Count && _sorted[_nextIndex].time <= _time)
        {
            var ev = _sorted[_nextIndex];
            _nextIndex++;

            if (verboseLogs)
                Debug.Log($"[EventSequencer] t={_time:0.000} fired '{ev.eventKey}' (scheduled {ev.time:0.000})", this);

            GameEventBus.Broadcast(ev.eventKey, _time, this);
        }
    }

    private int FindFirstEventIndexAtOrAfter(float t)
    {
        // Linear is fine for small lists; if you expect big lists, replace with binary search.
        for (int i = 0; i < _sorted.Count; i++)
            if (_sorted[i].time >= t)
                return i;
        return _sorted.Count;
    }
}
