// Assets/Scripts/Events/GameEventBus.cs
using System;
using UnityEngine;

public static class GameEventBus
{
    public readonly struct GameEvent
    {
        public readonly string key;
        public readonly float sequenceTime;
        public readonly UnityEngine.Object source;

        public GameEvent(string key, float sequenceTime, UnityEngine.Object source)
        {
            this.key = key;
            this.sequenceTime = sequenceTime;
            this.source = source;
        }
    }

    public static event Action<GameEvent> OnEvent;

    public static void Broadcast(string key, float sequenceTime, UnityEngine.Object source)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        OnEvent?.Invoke(new GameEvent(key, sequenceTime, source));
    }
}
