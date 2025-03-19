using UnityEngine;

namespace CozyFramework
{
    public class CozyManagerRoot : MonoBehaviour
    {
        private static CozyManagerRoot instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.Log($"[CozyManagerRoot] Destroying duplicate manager root: {gameObject.name}");
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[CozyManagerRoot] Initialized root manager: {gameObject.name}");
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
} 