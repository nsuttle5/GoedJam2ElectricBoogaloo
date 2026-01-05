using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Event Sequence", fileName = "EventSequence_")]
public sealed class EventSequenceAsset : ScriptableObject
{
    [Serializable]
    public struct TimedEvent
    {
        [Min(0f)] public float time;   
        public string eventKey;
        [TextArea(1, 3)]
        public string note;
    }

    [Header("Config (Additive)")]
    public List<TimedEvent> additiveEvents = new();

    [Header("Runtime (Absolute)")]
    public List<TimedEvent> events = new();

#if UNITY_EDITOR
    private void OnValidate()
    {
        RebuildRuntimeEvents();
    }
#endif

    public void RebuildRuntimeEvents()
    {
        events ??= new List<TimedEvent>();
        additiveEvents ??= new List<TimedEvent>();
        events.Clear();

        float t = 0f;
        for (int i = 0; i < additiveEvents.Count; i++)
        {
            var src = additiveEvents[i];

            float dt = Mathf.Max(0f, src.time);
            t += dt;
            events.Add(new TimedEvent
            {
                time = t,
                eventKey = src.eventKey,
                note = src.note
            });
        }
    }
}
