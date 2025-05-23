using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VidPlayer : MonoBehaviour
{
    [SerializeField] string videoFileName;
    [SerializeField] Image loadingScreen; // Reference to a loading screen image
    private VideoPlayer videoPlayer;
    private bool isVideoReady = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer)
        {
            // Subscribe to video player events
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.errorReceived += OnVideoError;
            
            // Show loading screen
            if (loadingScreen != null)
                loadingScreen.gameObject.SetActive(true);

            // Start preparing the video
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
            videoPlayer.url = videoPath;
            videoPlayer.Prepare();
        }
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        isVideoReady = true;
        // Hide loading screen
        if (loadingScreen != null)
            loadingScreen.gameObject.SetActive(false);
        // Play the video
        videoPlayer.Play();
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("Video Error: " + message);
        // Hide loading screen on error
        if (loadingScreen != null)
            loadingScreen.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
