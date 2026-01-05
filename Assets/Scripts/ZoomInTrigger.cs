using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class ZoomInTrigger : MonoBehaviour
{
    public string listenForKey = "Meow4";
    public string key2;
    public CinemachineCamera cam;
    public float zoomSpeed = 5;
    public float maxZoomIn = 30;

    private void OnEnable() => GameEventBus.OnEvent += Handle;
    private void OnDisable() => GameEventBus.OnEvent -= Handle;

    private void Handle(GameEventBus.GameEvent ev)
    {
        if (ev.key != listenForKey && ev.key != key2) return;

        if(ev.key == listenForKey)
        {
            Debug.Log($"[{name}] received '{ev.key}' at seq t={ev.sequenceTime:0.000}", this);
            StartCoroutine(zoom());
        } else
        {
            StopAllCoroutines();
        }
        
    }

    public IEnumerator zoom()
    {
        float currentFOV = cam.Lens.FieldOfView;

        cam.Lens.FieldOfView = Mathf.Lerp(currentFOV, maxZoomIn, zoomSpeed);
        yield return new WaitForSeconds(0.025f);

        StartCoroutine(zoom());
    }
}
