using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Platformer.Gameplay;

public class EndScreenSetup : MonoBehaviour
{
    [Header("High Score UI")]
    [SerializeField] private GameObject highScorePanel;
    [SerializeField] private Transform highScoreContainer;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private GameObject highScoreEntryPrefab;
    
    [Header("UI Elements")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    private void Awake()
    {
        // Make sure the HighScoreManager exists
        if (HighScoreManager.Instance == null)
        {
            GameObject highScoreManagerObj = new GameObject("HighScoreManager");
            highScoreManagerObj.AddComponent<HighScoreManager>();
        }
        
        // Add HighScoreUI component if it doesn't exist
        HighScoreUI highScoreUI = GetComponent<HighScoreUI>();
        if (highScoreUI == null)
        {
            highScoreUI = gameObject.AddComponent<HighScoreUI>();
        }
        
        // Set up the references
        if (highScoreUI != null)
        {
            // Use reflection to set the private fields
            System.Type type = highScoreUI.GetType();
            
            if (highScorePanel != null)
                type.GetField("highScorePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(highScoreUI, highScorePanel);
                
            if (highScoreContainer != null)
                type.GetField("highScoreContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(highScoreUI, highScoreContainer);
                
            if (playerNameInput != null)
                type.GetField("playerNameInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(highScoreUI, playerNameInput);
                
            if (submitButton != null)
                type.GetField("submitButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(highScoreUI, submitButton);
                
            if (finalScoreText != null)
                type.GetField("finalScoreText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(highScoreUI, finalScoreText);
                
            if (highScoreEntryPrefab != null)
                type.GetField("highScoreEntryPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(highScoreUI, highScoreEntryPrefab);
        }
        
        // Set up button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }
    
    private void RestartGame()
    {
        // Reset the game over state
        PlayerDeath.ResetGameOverState();
        
        // Reset time scale
        Time.timeScale = 1;
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void GoToMainMenu()
    {
        // Reset the game over state
        PlayerDeath.ResetGameOverState();
        
        // Reset time scale
        Time.timeScale = 1;
        
        // Load the main menu scene
        SceneManager.LoadScene("StartScreen");
    }
}

/*
INSTRUCTIONS FOR SETTING UP THE HIGH SCORE UI:

1. Add this script to your ENDSCREEN Canvas GameObject.

2. Create the following UI elements as children of the ENDSCREEN Canvas:
   - A Panel named "HighScorePanel" for the high score display
   - A Vertical Layout Group named "HighScoreContainer" inside the panel
   - A TMP_InputField for the player name input
   - A Button for submitting the high score
   - A TextMeshProUGUI element for displaying the final score

3. Create a prefab for the high score entry as described in the HighScoreEntry.prefab.txt file.

4. Assign all these elements to the corresponding fields in the Inspector.

5. Make sure you have buttons for restarting the game and returning to the main menu.

6. Add the HighScoreManager to your scene or create a prefab for it.

This setup will allow players to enter their name when they achieve a high score and view the leaderboard.
*/ 