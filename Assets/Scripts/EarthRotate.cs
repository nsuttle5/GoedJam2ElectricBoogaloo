using UnityEngine;

public class EarthRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Speed of rotation in degrees per second")]
    public float rotationSpeed = 30f;
    
    [Tooltip("Axis of rotation (typically Vector3.up for Y-axis)")]
    public Vector3 rotationAxis = Vector3.up;
    
    [Header("Texture Settings")]
    [Tooltip("Texture for the first time scene loads (normal Earth)")]
    public Texture normalEarthTexture;
    
    [Tooltip("Texture for subsequent loads (desolate Earth)")]
    public Texture desolateEarthTexture;
    
    private const string SCENE_LOADED_KEY = "EarthSceneLoaded";
    private Renderer earthRenderer;

    void Start()
    {
        // Get the renderer component
        earthRenderer = GetComponent<Renderer>();
        
        if (earthRenderer == null)
        {
            Debug.LogError("EarthRotate: No Renderer component found on " + gameObject.name);
            return;
        }
        
        // Check if this is the first time loading the scene
        bool hasLoadedBefore = PlayerPrefs.GetInt(SCENE_LOADED_KEY, 0) == 1;
        
        if (hasLoadedBefore)
        {
            // Apply desolate Earth texture
            if (desolateEarthTexture != null)
            {
                earthRenderer.material.mainTexture = desolateEarthTexture;
            }
            else
            {
                Debug.LogWarning("EarthRotate: Desolate Earth texture is not assigned!");
            }
        }
        else
        {
            // Apply normal Earth texture
            if (normalEarthTexture != null)
            {
                earthRenderer.material.mainTexture = normalEarthTexture;
            }
            else
            {
                Debug.LogWarning("EarthRotate: Normal Earth texture is not assigned!");
            }
            
            // Mark that the scene has been loaded
            PlayerPrefs.SetInt(SCENE_LOADED_KEY, 1);
            PlayerPrefs.Save();
        }
    }

    void Update()
    {
        // Rotate the Earth around the specified axis
        transform.Rotate(rotationAxis.normalized * rotationSpeed * Time.deltaTime);
    }
}
