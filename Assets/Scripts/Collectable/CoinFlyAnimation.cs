using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CoinFlyAnimation : MonoBehaviour
{
    [Header("Coin Animation")]
    [SerializeField] private GameObject flyingCoinPrefab;
    [SerializeField] private Transform coinUITarget;
    
    [Header("Life Animation")]
    [SerializeField] private GameObject flyingLifePrefab;
    [SerializeField] private Transform livesUITarget;
    
    [Header("Animation Settings")]
    [SerializeField] private float flyDuration = 0.5f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float pulseScale = 2.0f;
    [SerializeField] private float pulseDuration = 0.2f;
    
    private Canvas mainCanvas;
    private Coroutine coinPulseCoroutine;
    private Coroutine lifesPulseCoroutine;
    private Vector3 originalCoinTargetScale;
    private Vector3 originalLifeTargetScale;

    private void Start()
    {
        mainCanvas = FindFirstObjectByType<Canvas>();
        
        if (coinUITarget != null)
        {
            originalCoinTargetScale = coinUITarget.localScale;
        }
        if (livesUITarget != null)
        {
            originalLifeTargetScale = livesUITarget.localScale;
        }
    }

    public void StartCoinFlyAnimation(Vector3 worldPosition)
    {
        if (flyingCoinPrefab == null || coinUITarget == null)
        {
            Debug.LogWarning("Missing coin prefab or target for animation");
            return;
        }
        StartCoroutine(AnimateCoinFly(worldPosition));
    }

    public void StartLifeFlyAnimation(Vector3 worldPosition)
    {
        if (flyingLifePrefab == null || livesUITarget == null)
        {
            Debug.LogWarning("Missing life prefab or target for animation");
            return;
        }
        StartCoroutine(AnimateLifeFly(worldPosition));
    }

    private IEnumerator AnimateCoinFly(Vector3 startWorldPos)
    {
        GameObject uiObject = Instantiate(flyingCoinPrefab, mainCanvas.transform);
        RectTransform rect = uiObject.GetComponent<RectTransform>();

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(startWorldPos);
        rect.position = screenPosition;
        Vector2 endPosition = coinUITarget.position;
        
        yield return AnimateObject(rect, screenPosition, endPosition);
        
        Destroy(uiObject);
        
        if (coinPulseCoroutine != null)
        {
            StopCoroutine(coinPulseCoroutine);
        }
        coinPulseCoroutine = StartCoroutine(PulseCoinUI());
    }

    private IEnumerator AnimateLifeFly(Vector3 startWorldPos)
    {
        GameObject uiObject = Instantiate(flyingLifePrefab, mainCanvas.transform);
        RectTransform rect = uiObject.GetComponent<RectTransform>();

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(startWorldPos);
        rect.position = screenPosition;
        Vector2 endPosition = livesUITarget.position;
        
        yield return AnimateObject(rect, screenPosition, endPosition);
        
        Destroy(uiObject);
        
        if (lifesPulseCoroutine != null)
        {
            StopCoroutine(lifesPulseCoroutine);
        }
        lifesPulseCoroutine = StartCoroutine(PulseLifeUI());
    }

    private IEnumerator AnimateObject(RectTransform rect, Vector2 startPosition, Vector2 endPosition)
    {
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / flyDuration;
            
            float curveValue = speedCurve.Evaluate(normalizedTime);
            rect.position = Vector2.Lerp(startPosition, endPosition, curveValue);

            float scale = 1f - (0.5f * curveValue);
            rect.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        rect.position = endPosition;
    }

    private IEnumerator PulseCoinUI()
    {
        yield return PulseUI(coinUITarget, originalCoinTargetScale);
        coinPulseCoroutine = null;
    }

    private IEnumerator PulseLifeUI()
    {
        yield return PulseUI(livesUITarget, originalLifeTargetScale);
        lifesPulseCoroutine = null;
    }

    private IEnumerator PulseUI(Transform target, Vector3 originalScale)
    {
        float elapsed = 0f;
        Vector3 startScale = target.localScale;
        Vector3 maxScale = originalScale * pulseScale;

        // Scale up
        while (elapsed < pulseDuration / 2)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / (pulseDuration / 2);
            target.localScale = Vector3.Lerp(startScale, maxScale, normalizedTime);
            yield return null;
        }

        elapsed = 0f;

        // Scale down
        while (elapsed < pulseDuration / 2)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / (pulseDuration / 2);
            target.localScale = Vector3.Lerp(maxScale, originalScale, normalizedTime);
            yield return null;
        }

        target.localScale = originalScale;
    }

    private void OnDisable()
    {
        if (coinPulseCoroutine != null)
        {
            StopCoroutine(coinPulseCoroutine);
            if (coinUITarget != null)
            {
                coinUITarget.localScale = originalCoinTargetScale;
            }
        }
        
        if (lifesPulseCoroutine != null)
        {
            StopCoroutine(lifesPulseCoroutine);
            if (livesUITarget != null)
            {
                livesUITarget.localScale = originalLifeTargetScale;
            }
        }
    }
}