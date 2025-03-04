using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HighScoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private GameObject highScoreEntryPrefab;
    [SerializeField] private Transform highScoreContainer;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    
    [Header("Settings")]
    [SerializeField] private int maxDisplayedScores = 10;
    
    private int currentScore;
    private bool hasSubmittedScore = false;
    
    private void OnEnable()
    {
        // Get the current score when the end screen is enabled
        if (ScoreManager.Instance != null)
        {
            currentScore = ScoreManager.Instance.GetScore();
            
            // Display the final score
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + currentScore;
            }
            
            // Check if this is a high score
            if (HighScoreManager.Instance != null && HighScoreManager.Instance.IsHighScore(currentScore))
            {
                // Show the input field for the player's name
                ShowNameInputPanel(true);
            }
            else
            {
                // Just show the high scores
                ShowNameInputPanel(false);
                DisplayHighScores();
            }
        }
    }
    
    private void Start()
    {
        // Set up the submit button
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(SubmitHighScore);
        }
    }
    
    private void ShowNameInputPanel(bool show)
    {
        if (playerNameInput != null)
        {
            playerNameInput.gameObject.SetActive(show);
        }
        
        if (submitButton != null)
        {
            submitButton.gameObject.SetActive(show);
        }
    }
    
    public void SubmitHighScore()
    {
        if (hasSubmittedScore || HighScoreManager.Instance == null)
            return;
            
        string playerName = playerNameInput.text;
        
        // Use a default name if the player didn't enter one
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player";
        }
        
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
        if (HighScoreManager.Instance == null || highScoreContainer == null || highScoreEntryPrefab == null)
            return;
            
        // Clear existing entries
        foreach (Transform child in highScoreContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get the high scores
        List<HighScoreEntry> highScores = HighScoreManager.Instance.GetHighScores();
        
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
            }
        }
    }
} 