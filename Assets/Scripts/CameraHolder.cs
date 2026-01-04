using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    public CinemachineBrain brain;
    public CutsceneManager CM;
    public List<CinemachineCamera> cameraList;
    public List<float> shotLengths;
    public string nextScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraList[0].Prioritize();

        CM = GameObject.Find("CutsceneManager").GetComponentInChildren<CutsceneManager>();
        CM.cameraHolder = this;



        CM.canMoveOn = false;
        CM.cameraOn = 0;
        CM.startScenes();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
