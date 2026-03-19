using UnityEngine;
using UnityEngine.SceneManagement;
using CozyFramework;

public class StartGame : MonoBehaviour
{
    public void LoadLevel(string sceneName)
    {
        if (PlayerManager.Instance != null && !PlayerManager.Instance.PlayerHasUsername)
        {
            if (CozyPickUsername.Instance != null)
                CozyPickUsername.Instance.ShowPickUsernameView();
            return;
        }

        SceneManager.LoadScene(sceneName);

        GameObject cozyManager = GameObject.Find("CozyManager");
        if (cozyManager != null)
        {
            GameObject canvas = cozyManager.transform.Find("Canvas").gameObject;
            if (canvas != null)
            {
                canvas.SetActive(false);
            }
        }
    }
}
