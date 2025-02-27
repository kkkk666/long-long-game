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
    public int lives = 3;
    private Coroutine countingCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Register to the scene loaded event
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Reset score and lives when a new scene is loaded
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Reset score and lives
        ResetScoreManager();
    }
    
    // Reset the score manager to initial values
    public void ResetScoreManager()
    {
       
        score = 0;
        displayedScore = 0;
        lives = 3; // Reset to default lives
        
        // Update displays
        UpdateScoreDisplay(score);
        UpdateLivesDisplay();
        
        // Stop any counting coroutine
        if (countingCoroutine != null)
        {
            StopCoroutine(countingCoroutine);
            countingCoroutine = null;
        }
    }
    
    private void OnDestroy()
    {
        // Unregister from the scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
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