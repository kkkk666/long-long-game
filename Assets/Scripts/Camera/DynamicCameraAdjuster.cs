using UnityEngine;
using Unity.Cinemachine;

public class DynamicCameraAdjuster : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float baseOrthographicSize = 3.5f;
    [SerializeField] private float minScreenWidth = 800f;
    [SerializeField] private float maxOrthographicSize = 5f;
    
    private CinemachineCamera virtualCamera;
    private float aspectRatio;
    private float lastScreenWidth;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("No CinemachineCamera component found!");
            enabled = false;
            return;
        }

        lastScreenWidth = Screen.width;
        AdjustCameraForScreenSize();
    }

    private void Update()
    {
        // Only adjust if screen width has changed significantly
        if (Mathf.Abs(Screen.width - lastScreenWidth) > 10f)
        {
            lastScreenWidth = Screen.width;
            AdjustCameraForScreenSize();
        }
    }

    private void AdjustCameraForScreenSize()
    {
        if (virtualCamera == null) return;

        // Calculate aspect ratio
        aspectRatio = (float)Screen.width / Screen.height;

        // Adjust orthographic size based on screen width
        float targetOrthographicSize = baseOrthographicSize;
        
        if (Screen.width < minScreenWidth)
        {
            // Increase orthographic size for smaller screens
            float scaleFactor = minScreenWidth / Screen.width;
            targetOrthographicSize = Mathf.Min(baseOrthographicSize * scaleFactor, maxOrthographicSize);
        }

        // Apply the new orthographic size
        virtualCamera.Lens.OrthographicSize = targetOrthographicSize;

        // Get the composer component from the virtual camera's pipeline
        var composer = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineRotationComposer;
        if (composer != null)
        {
            // Increase dead zone for smaller screens
            float deadZoneSize = Mathf.Max(0.1f, 0.15f * (minScreenWidth / Screen.width));
            composer.Composition.DeadZone.Size = new Vector2(deadZoneSize, deadZoneSize);

            // Adjust hard limits by adjusting soft zone (which effectively acts as hard limits)
            float hardLimitSize = Mathf.Min(0.9f, 0.8f * (Screen.width / minScreenWidth));
            composer.Composition.HardLimits.Size = new Vector2(hardLimitSize, hardLimitSize);
        }
    }
} 