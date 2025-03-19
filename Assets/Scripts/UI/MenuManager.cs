using UnityEngine;
using UnityEngine.SceneManagement;
using CozyFramework;



public class MenuManager : MonoBehaviour
{
    public void LoadStartScreen()
    {
        SceneManager.LoadScene("StartScreen"); // Use your start screen scene name
         GameObject cozyManager = GameObject.Find("CozyManager");
        if (cozyManager != null)
        {
           //get the child object named Canvas
           GameObject canvas = cozyManager.transform.Find("Canvas").gameObject;
           if (canvas != null)
           {
            //set the canvas to active
            canvas.SetActive(true);
           }

        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}