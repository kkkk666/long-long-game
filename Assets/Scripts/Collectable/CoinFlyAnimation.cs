using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CoinFlyAnimation : MonoBehaviour
{
    [SerializeField] private GameObject flyingCoinPrefab;
    [SerializeField] private Transform coinUITarget;
    [SerializeField] private float flyDuration = 0.5f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float pulseScale = 2.0f;
    [SerializeField] private float pulseDuration = 0.2f;
    
    private Canvas mainCanvas;
    private Coroutine pulseCoroutine;
    private Vector3 originalTargetScale;

    private void Start()
    {
        mainCanvas = FindFirstObjectByType<Canvas>();
        if (coinUITarget != null)
        {
            originalTargetScale = coinUITarget.localScale;
        }
    }

    public void StartCoinFlyAnimation(Vector3 worldPosition)
    {
        StartCoroutine(AnimateCoinFly(worldPosition));
    }

    private IEnumerator AnimateCoinFly(Vector3 startWorldPos)
    {
        GameObject coinUI = Instantiate(flyingCoinPrefab, mainCanvas.transform);
        RectTransform coinRect = coinUI.GetComponent<RectTransform>();

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(startWorldPos);
        coinRect.position = screenPosition;

        Vector2 endPosition = coinUITarget.position;

        float elapsed = 0f;
        Vector2 startPosition = screenPosition;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / flyDuration;
            
            float curveValue = speedCurve.Evaluate(normalizedTime);
            coinRect.position = Vector2.Lerp(startPosition, endPosition, curveValue);

            float scale = 1f - (0.5f * curveValue);
            coinRect.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        coinRect.position = endPosition;
        Destroy(coinUI);
        
        // Start new pulse animation, interrupting any existing one
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        pulseCoroutine = StartCoroutine(PulseAnimation());
    }

    private IEnumerator PulseAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = coinUITarget.localScale;
        
        // Calculate target scale based on original scale
        Vector3 maxScale = originalTargetScale * pulseScale;

        // Scale up
        while (elapsed < pulseDuration / 2)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / (pulseDuration / 2);
            coinUITarget.localScale = Vector3.Lerp(startScale, maxScale, normalizedTime);
            yield return null;
        }

        elapsed = 0f;

        // Scale down
        while (elapsed < pulseDuration / 2)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / (pulseDuration / 2);
            coinUITarget.localScale = Vector3.Lerp(maxScale, originalTargetScale, normalizedTime);
            yield return null;
        }

        // Always reset to original scale
        coinUITarget.localScale = originalTargetScale;
        pulseCoroutine = null;
    }

    // Optional: Add this method to ensure proper cleanup if the object is destroyed
    private void OnDisable()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            if (coinUITarget != null)
            {
                coinUITarget.localScale = originalTargetScale;
            }
        }
    }
}