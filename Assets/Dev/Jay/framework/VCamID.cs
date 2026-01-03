using UnityEngine;
using Unity.Cinemachine;

[DisallowMultipleComponent]
public sealed class VCamID : MonoBehaviour
{
    [Tooltip("Stable ID used by CutsceneManager (do NOT rely on GameObject name).")]
    public string id;

    [Tooltip("If assigned, this is used. Otherwise we try GetComponent<CinemachineCamera>().")]
    public CinemachineCamera cameraOverride;

    public CinemachineCamera GetCamera()
    {
        if (cameraOverride != null) return cameraOverride;
        return GetComponent<CinemachineCamera>();
    }
}
