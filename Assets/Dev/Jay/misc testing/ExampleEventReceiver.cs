using UnityEngine;

public class ExampleEventReceiver : MonoBehaviour
{
    public string listenForKey = "Meow4";

    private void OnEnable() => GameEventBus.OnEvent += Handle;
    private void OnDisable() => GameEventBus.OnEvent -= Handle;






    private void Handle(GameEventBus.GameEvent ev)
    {
        if (ev.key != listenForKey) return;

        Debug.Log($"[{name}] received '{ev.key}' at seq t={ev.sequenceTime:0.000}", this);
    }
}
