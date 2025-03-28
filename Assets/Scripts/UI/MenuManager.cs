using UnityEngine;
using UnityEngine.SceneManagement;
using CozyFramework;

public class MenuManager : MonoBehaviour
{
    public void LoadStartScreen()
    {
        // Find and deactivate the ENDSCREEN if it exists
        GameObject cozyManager = GameObject.Find("CozyManager");
        if (cozyManager != null)
        {
            Transform endScreenTransform = cozyManager.transform.Find("ENDSCREEN");
            if (endScreenTransform != null)
            {
                endScreenTransform.gameObject.SetActive(false);
            }
        }

        // Load the start screen scene
        SceneManager.LoadScene("StartScreen");
        
        // Reactivate the CozyManager's Canvas
        cozyManager = GameObject.Find("CozyManager");
        if (cozyManager != null)
        {
            GameObject canvas = cozyManager.transform.Find("Canvas").gameObject;
            if (canvas != null)
            {
                canvas.SetActive(true);
            }
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}