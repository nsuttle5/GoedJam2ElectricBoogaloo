// Assets/Scripts/Events/EventSequenceAsset.cs
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

    [Tooltip("Events are fired when the sequencer time passes their 'time'.")]
    public List<TimedEvent> events = new();
}
