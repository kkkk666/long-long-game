using UnityEngine;
using Unity.Cinemachine;
using Platformer.Mechanics;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CinemachineBrain))]
public class CameraSpeedAdjuster : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("The Cinemachine Virtual Camera to adjust")]
    public CinemachineCamera virtualCamera;
    
    [Tooltip("How far to shift the camera's tracking position based on speed")]
    public float maxHorizontalOffset = 3f;
    
    [Tooltip("The base player speed at which offset starts increasing")]
    public float baseSpeed = 7f;
    
    [Tooltip("The maximum player speed at which offset reaches its maximum")]
    public float maxSpeed = 15f;
    
    [Tooltip("How smoothly to adjust the camera position")]
    public float smoothTime = 0.3f;

    private CinemachinePositionComposer framingTransposer;
    private PlayerController playerController;
    private Vector2 defaultTrackedObjectOffset;
    private Vector2 currentVelocity;

    [System.Obsolete]
    private void Start()
    {
        // Get the framing transposer from the virtual camera
        if (virtualCamera != null)
        {
            framingTransposer = virtualCamera.GetComponent<CinemachinePositionComposer>();
            if (framingTransposer != null)
            {
                // Store the default tracked object offset
                defaultTrackedObjectOffset = framingTransposer.Composition.ScreenPosition;
            }
            else
            {
                Debug.LogError("No CinemachinePositionComposer found on the virtual camera! Make sure you have this component added.");
            }
        }
        else
        {
            Debug.LogError("Virtual Camera not assigned! Please assign it in the inspector.");
        }

        // Get the player controller
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("No PlayerController found in the scene!");
            return;
        }

        // Set initial values from player controller
        baseSpeed = playerController.baseSpeed;
        maxSpeed = playerController.maxSpeed;
    }

    private void Update()
{
    if (framingTransposer != null && playerController != null)
    {
        // Calculate target offset based on current speed
        float speedRatio = Mathf.Clamp01((playerController.currentSpeed - baseSpeed) / (maxSpeed - baseSpeed));
        Vector2 targetPosition = defaultTrackedObjectOffset + (Vector2.right * maxHorizontalOffset * speedRatio);

        // Smoothly adjust the screen position
        var composition = framingTransposer.Composition;
        composition.ScreenPosition = Vector2.SmoothDamp(
            composition.ScreenPosition,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
        framingTransposer.Composition = composition;
    }
}

    // Optional: Visualize the camera offset in the editor
    private void OnDrawGizmosSelected()
    {
        if (virtualCamera != null && virtualCamera.Follow != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 basePosition = virtualCamera.Follow.position;
            Vector3 maxOffset = basePosition + Vector3.right * maxHorizontalOffset;
            Gizmos.DrawLine(basePosition, maxOffset);
            Gizmos.DrawWireSphere(maxOffset, 0.5f);
        }
    }

#if UNITY_EDITOR
    // Add a menu item to create the camera adjuster
    [MenuItem("GameObject/Cinemachine/Speed-Based Camera Adjuster", false, 11)]
    static void CreateCameraAdjuster()
    {
        GameObject go = new GameObject("CM Speed Adjuster");
        go.AddComponent<CameraSpeedAdjuster>();
        Selection.activeGameObject = go;
        
        // Position the adjuster at the scene view camera position
        SceneView view = SceneView.lastActiveSceneView;
        if (view != null)
            go.transform.position = view.camera.transform.position;
        else
            go.transform.position = Vector3.zero;
    }
#endif
}