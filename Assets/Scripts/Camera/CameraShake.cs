using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    
    [SerializeField] private float shakeDuration = 1f;
    [SerializeField] private float shakeAmplitude = 1f;
    
    private CinemachineCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            virtualCamera = GetComponent<CinemachineCamera>();
            if (virtualCamera != null)
            {
                noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
                if (noise == null)
                {
                    enabled = false;
                    return;
                }
            }
            else
            {
                enabled = false;
                return;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShakeCamera()
    {
        if (noise == null)
        {
            noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise == null)
            {
                return;
            }
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private System.Collections.IEnumerator ShakeCoroutine()
    {
        if (noise == null)
        {
            yield break;
        }

        noise.enabled = true;
        noise.AmplitudeGain = shakeAmplitude;

        yield return new WaitForSeconds(shakeDuration);

        noise.enabled = false;
        noise.AmplitudeGain = 0f;
        
        shakeCoroutine = null;
    }
} 