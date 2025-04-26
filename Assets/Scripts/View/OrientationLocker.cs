using UnityEngine;

public class OrientationLocker : MonoBehaviour
{
    [SerializeField] private bool persistAcrossScenes = true;
    private static OrientationLocker instance;
    private bool jsInjected = false;

    void Awake()
    {
        // Implement singleton pattern if needed across scenes
        if (persistAcrossScenes)
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        // Set initial orientation
        SetLandscapeOrientation();
    }

    void Start()
    {
        // Inject JavaScript on start
        #if UNITY_WEBGL && !UNITY_EDITOR
            if (!jsInjected)
            {
                InjectJavaScript();
                jsInjected = true;
            }
        #endif
    }

    void SetLandscapeOrientation()
    {
        // For WebGL, we're using injected JavaScript instead of DllImport
        #if UNITY_WEBGL && !UNITY_EDITOR
            if (!jsInjected)
            {
                InjectJavaScript();
                jsInjected = true;
            }
            Application.ExternalEval("rotateToLandscape();");
        #endif
    }

    #if UNITY_WEBGL && !UNITY_EDITOR
    private void InjectJavaScript()
    {
        string js = @"
        function rotateToLandscape() {
            // Lock screen orientation if supported
            if (screen.orientation && screen.orientation.lock) {
                screen.orientation.lock('landscape').catch(function(error) {
                    console.error('Orientation lock failed: ' + error);
                });
            }
            
            // Add CSS for older browsers or as fallback
            var style = document.createElement('style');
            style.innerHTML = `
                canvas {
                    transform-origin: left top;
                }
                @media screen and (orientation: portrait) {
                    canvas {
                        transform: rotate(-90deg) translateX(-100%);
                        width: 100vh !important;
                        height: 100vw !important;
                        position: absolute;
                        top: 0;
                        left: 0;
                    }
                    body {
                        width: 100vh;
                        height: 100vw;
                        overflow: hidden;
                    }
                }
            `;
            document.head.appendChild(style);
            
            // Add a message for mobile users
            if (!document.getElementById('orientation-message')) {
                var msg = document.createElement('div');
                msg.id = 'orientation-message';
                msg.style.cssText = 'display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: black; color: white; text-align: center; font-size: 24px; z-index: 999; padding-top: 40%; font-family: Arial;';
                msg.innerHTML = 'Please rotate your device to landscape mode';
                document.body.appendChild(msg);
                
                // Show/hide message based on orientation
                function checkOrientation() {
                    if (window.innerWidth < window.innerHeight) {
                        msg.style.display = 'block';
                    } else {
                        msg.style.display = 'none';
                    }
                }
                
                window.addEventListener('resize', checkOrientation);
                checkOrientation();
            }
        }";
        
        Application.ExternalEval(js);
    }
    #endif

    void Update()
    {
        // Continuously check and enforce orientation in WebGL
        #if UNITY_WEBGL && !UNITY_EDITOR
        if (Screen.width < Screen.height)
        {
            SetLandscapeOrientation();
        }
        #endif
    }
}