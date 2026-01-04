using Unity.Cinemachine;
using UnityEngine;

public class RunDollyWhenActive : MonoBehaviour
{
    public CinemachineCamera cam;
    public CinemachineSplineDolly dolly;
    public float dollySpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(cam.IsLive)
        {
            var fixedSpeedDolly = dolly.AutomaticDolly.Method as SplineAutoDolly.FixedSpeed;

            fixedSpeedDolly.Speed = dollySpeed;
        }
    }
}
