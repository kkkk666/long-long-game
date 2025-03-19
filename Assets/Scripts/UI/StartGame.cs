using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management
using CozyFramework;
public class StartGame : MonoBehaviour
{
    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        // get the cozymanger scene object 
        GameObject cozyManager = GameObject.Find("CozyManager");
        if (cozyManager != null)
        {
           //get the child object named Canvas
           GameObject canvas = cozyManager.transform.Find("Canvas").gameObject;
           if (canvas != null)
           {
            //set the canvas to active
            canvas.SetActive(false);
           }
        
    }


    // get the cozymanger scene object 







    // // Alternative method if you prefer using scene index
    // public void LoadLevelByIndex(int sceneIndex)
    // {
    //     SceneManager.LoadScene(sceneIndex);
    // }
}
}