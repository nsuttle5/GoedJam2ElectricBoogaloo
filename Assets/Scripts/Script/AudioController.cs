using FMOD.Studio;
using UnityEngine;
using FMODUnity;

public class AudioController : MonoBehaviour
{
    /*
     * Singleton Insurance
     */
    public static AudioController instance { get; private set; }
    private void Awake()
    {
        if (instance != null )
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    
    private static EventInstance eventBackgroundMusic;
    
    public static void StartBGM()
    {
        eventBackgroundMusic = RuntimeManager.CreateInstance(AudioEvents.backgroundMusic);
        eventBackgroundMusic.start();
    }

    public static void PlaySFX(string audioEvent)
    {
        RuntimeManager.PlayOneShot(audioEvent);
    }
}