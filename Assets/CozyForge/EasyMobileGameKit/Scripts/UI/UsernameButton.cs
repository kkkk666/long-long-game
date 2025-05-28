using UnityEngine;
using UnityEngine.UI;

namespace CozyFramework
{
    public class UsernameButton : MonoBehaviour
    {
        private void Start()
        {
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(ShowUsernameEditor);
            }
        }

        public void ShowUsernameEditor()
        {
            if (CozyPickUsername.Instance != null)
            {
                CozyPickUsername.Instance.ShowPickUsernameView();
            }
            else
            {
                Debug.LogError("[UsernameButton] CozyPickUsername.Instance is null!");
            }
        }
    }
} 