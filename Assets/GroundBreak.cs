using UnityEngine;
using UnityEngine.VFX;

public class GroundBreak : MonoBehaviour
{
    public VisualEffect visualEffect;
    public bool grow;
    public float radius;
    public float radiusIncreaseSpeed;
    public float maxRadius = 500;

    public void Grow()
    {
        grow = true;
    }
    void FixedUpdate()
    {
        if(!grow) {
            radius = 0;
            return;
        }
        
        visualEffect.SetFloat("Radius", radius);
        radius += radiusIncreaseSpeed;
        visualEffect.SendEvent("Spawn");

        if(radius >= maxRadius)
        {
            grow = false;
        }
    }
}
