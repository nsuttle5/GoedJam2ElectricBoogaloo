using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }
    public CameraHolder cameraHolder;
    public bool canMoveOn = false;
    public int cameraOn = 0;
    public void Awake()
    {
        // Singleton pattern
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject); // prevent duplicates
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // For testing camera switching
        if(Input.GetKeyDown(KeyCode.Space))
        {
            cameraHolder.cameraList[cameraOn].Prioritize();

            cameraOn++;
            if(cameraOn >= cameraHolder.cameraList.Count)
            {
                cameraOn = 0;
            }
        }
    }

    public void startScenes()
    {
        StartCoroutine(runScene());
    }

    public IEnumerator runScene()
    {
        yield return new WaitForSeconds(cameraHolder.shotLengths[cameraOn]);

        cameraOn++;

        if(cameraOn == cameraHolder.cameraList.Count - 1)
        {



            cameraHolder.cameraList[cameraOn].Prioritize();

            StartCoroutine(loadNextScene());
            yield return new WaitForSeconds(cameraHolder.shotLengths[cameraOn]);



            canMoveOn = true;
        } else
        {
            cameraHolder.cameraList[cameraOn].Prioritize();

            StartCoroutine(runScene());
        }
        
    }

    public IEnumerator loadNextScene()
    {
        Debug.Log("started load");
        AsyncOperation nextScene = SceneManager.LoadSceneAsync(cameraHolder.nextScene);



        nextScene.allowSceneActivation = false;

        while(!canMoveOn && !nextScene.isDone)
        {
            Debug.Log("Not loaded yet");

            yield return null;
        }

        nextScene.allowSceneActivation = true;
    }

    
}
