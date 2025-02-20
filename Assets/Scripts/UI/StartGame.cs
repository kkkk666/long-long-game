using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class StartGame : MonoBehaviour
{
    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // // Alternative method if you prefer using scene index
    // public void LoadLevelByIndex(int sceneIndex)
    // {
    //     SceneManager.LoadScene(sceneIndex);
    // }
}