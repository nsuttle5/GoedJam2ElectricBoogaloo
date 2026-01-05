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
    
    // Always Running
    public static string backgroundMusic = "event:/bgm";
    
    public static string ambience = "event:/sfx/ambience";
    
    // 2D SFX
    public static string explosionRinging = "event:/sfx/explosion_ringing";
    
    // 3D SFX
    public static string doorOpen = "event:/sfx/door_open";
    
    public static string canvasOpen = "event:/sfx/canvas_open";
    
    public static string typing = "event:/sfx/typing";
    
    public static string nukeExplosion = "event:/sfx/nuke_explosion";
    
    public static string gunShot = "event:/sfx/gunshot";
    
    public static string flagRuffle = "event:/sfx/flag_ruffle";
    
    public static string crowdMurmor = "event:/sfx/crowd_murmor";
    
    public static string gunshotSingle = "event:/sfx/gunshot_single";
    
    public static string staticSFX = "event:/sfx/static";
    
}