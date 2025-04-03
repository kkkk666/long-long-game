using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("How much this object should move relative to the camera (0 = static, 1 = moves with camera)")]
    [Range(0f, 1f)]
    public float parallaxEffect = 0.5f;
    
    [Tooltip("Whether to maintain the object's Y position")]
    public bool lockYPosition = true;
    
    [Tooltip("Whether to maintain the object's X position")]
    public bool lockXPosition = false;
    
    [Tooltip("Whether to maintain the object's Z position")]
    public bool lockZPosition = true;

    private Vector3 startPosition;
    private Vector3 cameraStartPosition;
    private Camera mainCamera;

    private void Start()
    {
        // Store the initial positions
        startPosition = transform.position;
        mainCamera = Camera.main;
        cameraStartPosition = mainCamera.transform.position;
    }

    private void LateUpdate()
    {
        // Calculate how far the camera has moved from its start position
        Vector3 cameraMovement = mainCamera.transform.position - cameraStartPosition;
        
        // Calculate the parallax movement
        Vector3 parallaxMovement = new Vector3(
            lockXPosition ? 0f : cameraMovement.x * parallaxEffect,
            lockYPosition ? 0f : cameraMovement.y * parallaxEffect,
            lockZPosition ? 0f : cameraMovement.z * parallaxEffect
        );
        
        // Apply the parallax movement to the object's position
        transform.position = startPosition + parallaxMovement;
    }
} 