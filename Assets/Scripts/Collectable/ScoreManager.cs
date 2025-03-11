using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI jewelsText;
    [SerializeField] private float countDuration = 0.5f;
    [SerializeField] private float defaultFontSize = 36f; // Default font size for 1-2 digit numbers
    
    private int score = 0;
    private int displayedScore = 0;
    public int lives = 3;
    private int jewels = 0;
    private Coroutine countingCoroutine;
    private float currentFontSize;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Register to the scene loaded event
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Store the initial font size
            if (scoreText != null)
            {
                currentFontSize = scoreText.fontSize;
                defaultFontSize = currentFontSize;
            }
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
        jewels = 0;
        
        // Update displays
        UpdateScoreDisplay(score);
        UpdateLivesDisplay();
        UpdateJewelsDisplay();
        
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
        UpdateJewelsDisplay();
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
        if (scoreText != null)
        {
            scoreText.text = scoreToDisplay.ToString();
            
            // Calculate number of digits
            int digitCount = scoreToDisplay == 0 ? 1 : Mathf.FloorToInt(Mathf.Log10(scoreToDisplay)) + 1;
            
            // Adjust font size based on digit count
            if (digitCount > 2)
            {
                // Reduce font size by 25% for each additional digit beyond 2
                float scaleFactor = 1f / (1f + (digitCount - 2) * 0.25f);
                scoreText.fontSize = defaultFontSize * scaleFactor;
            }
            else
            {
                // Reset to default font size for 1-2 digits
                scoreText.fontSize = defaultFontSize;
            }
        }
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
        if (livesText != null)
            livesText.text = $"{lives}";
    }

    public void AddJewel()
    {
        jewels++;
        UpdateJewelsDisplay();
    }

    public int GetJewels()
    {
        return jewels;
    }

    private void UpdateJewelsDisplay()
    {
        if (jewelsText != null)
            jewelsText.text = $"{jewels}";
    }
}