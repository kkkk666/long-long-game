using UnityEngine;
using UnityEngine.SceneManagement;
using CozyFramework;

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