using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HighScoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private GameObject highScoreEntryPrefab;
    [SerializeField] private Transform highScoreContainer;
    
    [Header("Settings")]
    [SerializeField] private int maxDisplayedScores = 10;
    
    private int currentScore;
    private bool hasSubmittedScore = false;
    
    private void Awake()
    {
        Debug.Log("HighScoreUI Awake");
        
        // Validate components
        if (playerNameInput == null) Debug.LogError("playerNameInput is not assigned!");
        if (submitButton == null) Debug.LogError("submitButton is not assigned!");
        if (finalScoreText == null) Debug.LogError("finalScoreText is not assigned!");
        if (highScoreEntryPrefab == null) Debug.LogError("highScoreEntryPrefab is not assigned!");
        if (highScoreContainer == null) Debug.LogError("highScoreContainer is not assigned!");
    }
    
    private void OnEnable()
    {
        Debug.Log("HighScoreUI OnEnable");
        
        // Reset submission state
        hasSubmittedScore = false;
        
        // Get the current score when the end screen is enabled
        if (ScoreManager.Instance != null)
        {
            currentScore = ScoreManager.Instance.GetScore();
            Debug.Log($"Current score: {currentScore}");
            
            // Display the final score
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {currentScore}";
            }
            
            // Check if this is a high score
            if (HighScoreManager.Instance != null)
            {
                Debug.Log($"Checking if {currentScore} is a high score");
                if (HighScoreManager.Instance.IsHighScore(currentScore))
                {
                    Debug.Log("This is a high score! Showing input panel");
                    ShowNameInputPanel(true);
                }
                else
                {
                    Debug.Log("Not a high score. Hiding input panel");
                    ShowNameInputPanel(false);
                }
                
                // Always display high scores
                DisplayHighScores();
            }
            else
            {
                Debug.LogError("HighScoreManager.Instance is null!");
            }
        }
        else
        {
            Debug.LogError("ScoreManager.Instance is null!");
        }
    }
    
    private void Start()
    {
        Debug.Log("HighScoreUI Start");
        
        // Set up the submit button
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            submitButton.onClick.AddListener(SubmitHighScore);
            Debug.Log("Submit button listener added");
        }
    }
    
    private void ShowNameInputPanel(bool show)
    {
        if (playerNameInput != null)
        {
            playerNameInput.gameObject.SetActive(show);
            if (show)
            {
                playerNameInput.text = ""; // Clear any existing text
                playerNameInput.Select(); // Focus the input field
            }
        }
        
        if (submitButton != null)
        {
            submitButton.gameObject.SetActive(show);
        }
    }
    
    public void SubmitHighScore()
    {
        Debug.Log("SubmitHighScore called");
        
        if (hasSubmittedScore)
        {
            Debug.Log("Score already submitted");
            return;
        }
        
        if (HighScoreManager.Instance == null)
        {
            Debug.LogError("HighScoreManager.Instance is null!");
            return;
        }
            
        string playerName = playerNameInput != null ? playerNameInput.text : "Player";
        
        // Use a default name if the player didn't enter one
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player";
        }
        
        Debug.Log($"Submitting score: {playerName} - {currentScore}");
        
        // Add the high score
        HighScoreManager.Instance.AddHighScore(playerName, currentScore);
        
        // Hide the input panel
        ShowNameInputPanel(false);
        
        // Display the updated high scores
        DisplayHighScores();
        
        // Mark as submitted
        hasSubmittedScore = true;
    }
    
    public void DisplayHighScores()
    {
        Debug.Log("DisplayHighScores called");
        
        if (HighScoreManager.Instance == null)
        {
            Debug.LogError("HighScoreManager.Instance is null!");
            return;
        }
        
        if (highScoreContainer == null)
        {
            Debug.LogError("highScoreContainer is null!");
            return;
        }
        
        if (highScoreEntryPrefab == null)
        {
            Debug.LogError("highScoreEntryPrefab is null!");
            return;
        }
            
        // Clear existing entries
        foreach (Transform child in highScoreContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get the high scores
        List<HighScoreEntry> highScores = HighScoreManager.Instance.GetHighScores();
        Debug.Log($"Retrieved {highScores.Count} high scores");
        
        // Display each high score
        for (int i = 0; i < highScores.Count && i < maxDisplayedScores; i++)
        {
            HighScoreEntry entry = highScores[i];
            
            // Instantiate a new entry
            GameObject entryObject = Instantiate(highScoreEntryPrefab, highScoreContainer);
            
            // Set the rank, name, score, and date
            TextMeshProUGUI[] texts = entryObject.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 4)
            {
                texts[0].text = (i + 1).ToString(); // Rank
                texts[1].text = entry.playerName;   // Name
                texts[2].text = entry.score.ToString(); // Score
                texts[3].text = entry.date;         // Date
                Debug.Log($"Created high score entry: Rank {i + 1} - {entry.playerName} - {entry.score}");
            }
            else
            {
                Debug.LogError($"High score entry prefab does not have enough TextMeshProUGUI components! Found: {texts.Length}");
            }
        }
    }
} 