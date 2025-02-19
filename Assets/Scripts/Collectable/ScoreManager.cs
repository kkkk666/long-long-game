using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private float countDuration = 0.5f; // Duration of counting animation
    [SerializeField] private float updateStep = 1f; // How many numbers to count per step
    
    private int trueScore = 0;         // The actual score value
    private int displayedScore = 0;    // The currently displayed score
    private Coroutine countingCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreDisplay(displayedScore);
    }

    public void AddScore(int points)
    {
        // Update the true score immediately
        trueScore += points;
        
        // Stop any existing counting coroutine
        if (countingCoroutine != null)
        {
            StopCoroutine(countingCoroutine);
        }
        
        // Start new counting coroutine from current displayed score to true score
        countingCoroutine = StartCoroutine(CountTo(trueScore));
    }

    private IEnumerator CountTo(int target)
    {
        float elapsedTime = 0;
        float startScore = displayedScore;
        
        while (elapsedTime < countDuration)
        {
            elapsedTime += Time.deltaTime;
            float percentage = elapsedTime / countDuration;
            
            // Use Lerp to smoothly interpolate between displayed and target score
            displayedScore = Mathf.RoundToInt(Mathf.Lerp(startScore, target, percentage));
            UpdateScoreDisplay(displayedScore);
            
            yield return null;
        }
        
        // Ensure we end up exactly at the target score
        displayedScore = target;
        UpdateScoreDisplay(displayedScore);
    }

    private void UpdateScoreDisplay(int score)
    {
        scoreText.text = score.ToString();
    }

    // Helper method to get the true score (if needed)
    public int GetScore()
    {
        return trueScore;
    }
}