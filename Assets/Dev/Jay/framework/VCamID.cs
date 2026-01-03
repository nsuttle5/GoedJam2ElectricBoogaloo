using UnityEngine;
using Unity.Cinemachine;

[DisallowMultipleComponent]
public sealed class VCamID : MonoBehaviour
{
    [Tooltip("ID used by CutsceneManager2")]
    public string id;

    [Header("(ignore)")]
    public CinemachineCamera cameraOverride;



    public CinemachineCamera GetCamera()
    {
        if (cameraOverride != null) return cameraOverride;
        return GetComponent<CinemachineCamera>();
    }
}
