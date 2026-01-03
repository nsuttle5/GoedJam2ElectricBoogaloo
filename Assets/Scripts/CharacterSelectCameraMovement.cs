using UnityEngine;

public class CharacterSelectCameraMovement : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Maximum rotation angle in degrees (horizontal)")]
    [SerializeField] private float maxRotationX = 15f;
    
    [Tooltip("Maximum rotation angle in degrees (vertical)")]
    [SerializeField] private float maxRotationY = 10f;
    
    [Tooltip("How smoothly the camera rotates (lower = smoother)")]
    [SerializeField] private float smoothSpeed = 5f;
    
    private Quaternion initialRotation;
    private Vector3 targetRotation;

    void Start()
    {
        // Store the initial rotation to return to base position
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Get mouse position normalized to -1 to 1 range
        // (0,0) at bottom-left, (1,1) at top-right
        Vector2 mousePos = Input.mousePosition;
        Vector2 normalizedMouse = new Vector2(
            (mousePos.x / Screen.width) * 2f - 1f,   // -1 (left) to 1 (right)
            (mousePos.y / Screen.height) * 2f - 1f   // -1 (bottom) to 1 (top)
        );
        
        // Calculate target rotation based on mouse position
        // Invert Y rotation for natural feel (mouse up = look up)
        float targetYaw = normalizedMouse.x * maxRotationX;   // Horizontal rotation
        float targetPitch = -normalizedMouse.y * maxRotationY; // Vertical rotation
        
        targetRotation = new Vector3(targetPitch, targetYaw, 0f);
        
        // Smoothly interpolate to target rotation
        Quaternion targetQuat = initialRotation * Quaternion.Euler(targetRotation);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetQuat, Time.deltaTime * smoothSpeed);
    }
}
