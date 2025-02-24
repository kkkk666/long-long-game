using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private float countDuration = 0.5f;
    
    private int score = 0;
    private int displayedScore = 0;
    private int lives = 3;
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
        UpdateScoreDisplay(score);
        UpdateLivesDisplay();
    }

    public void AddScore(int points)
    {
        score += points;
        
        if (countingCoroutine != null)
        {
            StopCoroutine(countingCoroutine);
        }
        
        countingCoroutine = StartCoroutine(CountScore());
    }

    private IEnumerator CountScore()
    {
        float elapsed = 0f;
        int startScore = displayedScore;
        int targetScore = score;
        
        while (elapsed < countDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / countDuration;
            
            displayedScore = startScore + Mathf.RoundToInt((targetScore - startScore) * percent);
            UpdateScoreDisplay(displayedScore);
            
            yield return null;
        }
        
        displayedScore = targetScore;
        UpdateScoreDisplay(displayedScore);
        countingCoroutine = null;
    }

    private void UpdateScoreDisplay(int scoreToDisplay)
    {
        scoreText.text = scoreToDisplay.ToString();
    }

    public int GetScore()
    {
        return score;
    }

    public void AddLife(int amount = 1)
    {
        lives += amount;
        UpdateLivesDisplay();
    }

    public void RemoveLife()
    {
        lives--;
        UpdateLivesDisplay();
    }

    public int GetLives()
    {
        return lives;
    }

    private void UpdateLivesDisplay()
    {
        livesText.text = $"{lives}";
    }
}