using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathGraphicDisplay : MonoBehaviour
{
    [Header("Death Graphic Settings")]
    [SerializeField] private Image deathGraphic;
    [SerializeField] private Image darkBackground;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("Animation")]
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Color originalDarkBackgroundColor;
    
    private void Start()
    {
        // Store the original color of the dark background
        if (darkBackground != null)
        {
            originalDarkBackgroundColor = darkBackground.color;
            // Ensure the dark background starts transparent
            Color startColor = originalDarkBackgroundColor;
            startColor.a = 0f;
            darkBackground.color = startColor;
            darkBackground.gameObject.SetActive(true);
        }

        if (deathGraphic != null)
        {
            Color startColor = deathGraphic.color;
            startColor.a = 0f;
            deathGraphic.color = startColor;
            deathGraphic.gameObject.SetActive(true);
        }
    }

    public void ShowDeathGraphic()
    {
        if (deathGraphic == null || darkBackground == null) return;
        
        StartCoroutine(DeathGraphicSequence());
    }

    private IEnumerator DeathGraphicSequence()
    {
        // Fade in dark background first
        float elapsed = 0f;
        Color startColor = darkBackground.color;
        startColor.a = 0f;
        Color endColor = originalDarkBackgroundColor; // Use the original color with full alpha

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            float curveValue = fadeCurve.Evaluate(t);
            darkBackground.color = Color.Lerp(startColor, endColor, curveValue);
            yield return null;
        }

        // Fade in death graphic
        elapsed = 0f;
        startColor = deathGraphic.color;
        startColor.a = 0f;
        Color endGraphicColor = deathGraphic.color;
        endGraphicColor.a = 1f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            float curveValue = fadeCurve.Evaluate(t);
            deathGraphic.color = Color.Lerp(startColor, endGraphicColor, curveValue);
            yield return null;
        }

        // Display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out death graphic first
        elapsed = 0f;
        startColor = deathGraphic.color;
        endGraphicColor.a = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            float curveValue = fadeCurve.Evaluate(t);
            deathGraphic.color = Color.Lerp(startColor, endGraphicColor, curveValue);
            yield return null;
        }

        // Fade out dark background
        elapsed = 0f;
        startColor = darkBackground.color;
        Color endDarkColor = originalDarkBackgroundColor;
        endDarkColor.a = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            float curveValue = fadeCurve.Evaluate(t);
            darkBackground.color = Color.Lerp(startColor, endDarkColor, curveValue);
            yield return null;
        }

        // Ensure both are fully transparent
        Color finalColor = deathGraphic.color;
        finalColor.a = 0f;
        deathGraphic.color = finalColor;

        finalColor = originalDarkBackgroundColor;
        finalColor.a = 0f;
        darkBackground.color = finalColor;
    }
} 