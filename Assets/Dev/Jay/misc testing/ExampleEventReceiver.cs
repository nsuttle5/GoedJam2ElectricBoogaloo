using UnityEngine;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.Events;

public class ExampleEventReceiver : MonoBehaviour
{
    public string listenForKey = "Meow4";

    private void OnEnable() => GameEventBus.OnEvent += Handle;
    private void OnDisable() => GameEventBus.OnEvent -= Handle;

    public UnityEvent trigger;






    private void Handle(GameEventBus.GameEvent ev)
    {
        if (ev.key != listenForKey) return;

        trigger.Invoke();

        Debug.Log($"[{name}] received '{ev.key}' at seq t={ev.sequenceTime:0.000}", this);
    }
}
