using UnityEngine;
using UnityEngine.UI;

namespace CozyFramework
{
    public class UsernameEditButton : MonoBehaviour
    {
        private Button button;
        private CozyPickUsername cozyPickUsername;

        private void Awake()
        {
            // Get the button component
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("[UsernameEditButton] No Button component found!");
                return;
            }

            // Find CozyPickUsername in parent hierarchy
            cozyPickUsername = GetComponentInParent<CozyPickUsername>();
            if (cozyPickUsername == null)
            {
                Debug.LogError("[UsernameEditButton] CozyPickUsername not found in parent hierarchy!");
                return;
            }

            // Connect the button click to the username picker
            button.onClick.AddListener(OnEditButtonClicked);
            Debug.Log("[UsernameEditButton] Successfully connected to CozyPickUsername in parent");
        }

        private void OnEditButtonClicked()
        {
            if (cozyPickUsername != null)
            {
                cozyPickUsername.ShowPickUsernameView();
            }
            else
            {
                Debug.LogError("[UsernameEditButton] CozyPickUsername reference is null!");
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnEditButtonClicked);
            }
        }
    }
} 