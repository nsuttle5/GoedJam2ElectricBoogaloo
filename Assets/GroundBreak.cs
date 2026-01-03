using UnityEngine;
using UnityEngine.VFX;

public class GroundBreak : MonoBehaviour
{
    public VisualEffect visualEffect;
    public bool grow;
    public float radius;
    public float radiusIncreaseSpeed;
    void FixedUpdate()
    {
        if(!grow) {
            radius = 0;
            return;
        }
        
        visualEffect.SetFloat("Radius", radius);
        radius += radiusIncreaseSpeed;
        visualEffect.SendEvent("Spawn");
    }
}
