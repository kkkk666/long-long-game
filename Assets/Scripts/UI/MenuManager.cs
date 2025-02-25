using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LoadStartScreen()
    {
        SceneManager.LoadScene("StartScreen"); // Use your start screen scene name
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}