using UnityEngine;

public class AudioEvents : MonoBehaviour
{
    /*
     * Singleton Insurance
     */
    public static AudioEvents instance { get; private set; }
    private void Awake()
    {
        if (instance != null )
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    
    public static string backgroundMusic = "event:/bgm";
}