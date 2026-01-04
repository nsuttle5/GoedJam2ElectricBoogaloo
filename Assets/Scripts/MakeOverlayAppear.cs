using Unity.Cinemachine;
using UnityEngine;

public class MakeOverlayAppear : MonoBehaviour
{
    public GameObject overlay;
    public CinemachineCamera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(cam.IsLive)
        {
            overlay.SetActive(true);
        } else
        {
            overlay.SetActive(false);
        }
    }
}
