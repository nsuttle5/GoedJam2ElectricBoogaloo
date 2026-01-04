using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioOverlord : MonoBehaviour
{
    public List<string> audioSourceIds;
    public List<float> lengths;

    private int index = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator doTheAudio()
    {
        yield return new WaitForSeconds(lengths[index]);
        
        GameObject audioSource = GameObject.Find(audioSourceIds[index]);
        audioSource.SetActive(false);

        index++;

        if(!(index >= audioSourceIds.Count))
        {
            audioSource = GameObject.Find(audioSourceIds[index]);
            audioSource.SetActive(true);
            doTheAudio();
        }
    }
}
