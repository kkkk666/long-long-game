using UnityEngine;
using UnityEngine.UI; // Required for UI elements

public class OpenURLButton : MonoBehaviour
{
    [SerializeField] private string url = "https://example.com";
    
    void Start()
    {
        // Get the button component and add a listener
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OpenURL);
        }
    }
    
    void OpenURL()
    {
        // Open the URL in the default browser
        Application.OpenURL(url);
    }
}