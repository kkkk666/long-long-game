using UnityEngine;

namespace CozyFramework
{
    public class CozyManagerLoader : MonoBehaviour
    {
        [Header("CozyManager Configuration")]
        public GameObject CozyManagerPrefab;
        
        private static bool hasLoadedCozyManager = false;

        private void Awake()
        {
            LoadCozyManager();
        }

        private void LoadCozyManager()
        {
            // First check if a CozyManager already exists in the scene
            GameObject existingCozyManager = GameObject.Find("CozyManager");
            if (existingCozyManager != null)
            {
                Debug.Log("[CozyManagerLoader] CozyManager already exists in scene, skipping prefab load");
                hasLoadedCozyManager = true;
                return;
            }

            // Only load CozyManager once across all scenes
            if (!hasLoadedCozyManager)
            {
                if (CozyManagerPrefab != null)
                {
                    GameObject cozyManagerInstance = Instantiate(CozyManagerPrefab);
                    cozyManagerInstance.name = "CozyManager (Loaded)";
                    DontDestroyOnLoad(cozyManagerInstance);
                    hasLoadedCozyManager = true;
                    Debug.Log("[CozyManagerLoader] CozyManager loaded and set to DontDestroyOnLoad");
                }
                else
                {
                    Debug.LogError("[CozyManagerLoader] CozyManagerPrefab is not assigned!");
                }
            }
            else
            {
                Debug.Log("[CozyManagerLoader] CozyManager already loaded, skipping");
            }
        }
    }
} 