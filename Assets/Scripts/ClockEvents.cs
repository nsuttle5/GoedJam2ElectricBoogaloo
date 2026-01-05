using UnityEngine;

public class ClockEvents : MonoBehaviour
{
    public string secondSet = "Clock2";
    public string thirdSet = "Clock3";

    private void OnEnable() => GameEventBus.OnEvent += Handle;
    private void OnDisable() => GameEventBus.OnEvent -= Handle;

    private void Handle(GameEventBus.GameEvent ev)
    {
        if (ev.key != secondSet && ev.key != thirdSet) return;

        Debug.Log($"[{name}] received '{ev.key}' at seq t={ev.sequenceTime:0.000}", this);

        if(ev.key == secondSet)
        {
            GameObject.Find("Timer").GetComponent<StartTimer>().StartCountDown(30);
        } else
        {
            GameObject.Find("Timer").GetComponent<StartTimer>().isNegative = true;
            GameObject.Find("Timer").GetComponent<StartTimer>().StartCountDown(5);
        }
    }
}
