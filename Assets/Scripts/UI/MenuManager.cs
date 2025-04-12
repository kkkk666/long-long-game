using UnityEngine;
using UnityEngine.SceneManagement;
using CozyFramework;
using Platformer.Gameplay;
using TMPro;

public class MenuManager : MonoBehaviour
{
    private void Start()
    {
        // Register for scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unregister from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StartScreen")
        {
            // Find the CozyManager and handle its children
            GameObject cozyManager = GameObject.Find("CozyManager");
            if (cozyManager != null)
            {
                // Activate the Canvas
                Transform canvasTransform = cozyManager.transform.Find("Canvas");
                if (canvasTransform != null)
                {
                    canvasTransform.gameObject.SetActive(true);
                }

                // Deactivate the ENDSCREEN
                Transform endScreenTransform = cozyManager.transform.Find("ENDSCREEN");
                if (endScreenTransform != null)
                {
                    endScreenTransform.gameObject.SetActive(false);
                }
            }
            
            // Reset game state
            PlayerDeath.ResetGameOverState();
            Time.timeScale = 1;
        }
    }

    public void LoadStartScreen()
    {
        // Hide the high score text before loading the start screen
        GameObject cozyManager = GameObject.Find("CozyManager");
        if (cozyManager != null)
        {
            Transform endScreenTransform = cozyManager.transform.Find("ENDSCREEN");
            if (endScreenTransform != null)
            {
                Transform mainTransform = endScreenTransform.Find("Main");
                if (mainTransform != null)
                {
                    Transform highScoreTextTransform = mainTransform.Find("HighScoreText");
                    if (highScoreTextTransform != null)
                    {
                        // Clear the text before setting inactive
                        TextMeshProUGUI highScoreText = highScoreTextTransform.GetComponent<TextMeshProUGUI>();
                        if (highScoreText != null)
                        {
                            highScoreText.text = string.Empty;
                        }
                        highScoreTextTransform.gameObject.SetActive(false);
                    }
                }
            }
        }

        // Load start screen - canvas will be activated and end screen deactivated in OnSceneLoaded
        SceneManager.LoadScene("StartScreen");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}