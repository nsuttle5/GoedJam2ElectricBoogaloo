using UnityEngine;
using UnityEngine.VFX;

public class NukeGrow : MonoBehaviour
{
    public VisualEffect visualEffect;
    public bool grow;
    public float time;
    public float playRateMult = 1;
    public AnimationCurve curve;
    public float timeForCurve = 1;
    void FixedUpdate()
    {
        if(!grow) {
            visualEffect.playRate = 0;
            visualEffect.Reinit();
            time = 0;
            return;
        }

        visualEffect.playRate = playRateMult * curve.Evaluate(Mathf.Clamp(time/timeForCurve, 0, 1));
        visualEffect.SetFloat("Radius", curve.Evaluate(Mathf.Clamp(time/timeForCurve, 0, 1))*40);
        //visualEffect.SetFloat("startSpeed", time);
        time+= Time.deltaTime;
    }
}
