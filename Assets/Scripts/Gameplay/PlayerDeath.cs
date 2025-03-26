using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using ShadowGroveGames.WebhooksForDiscord.Scripts;
using CozyFramework; 

namespace Platformer.Gameplay
{
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        
        // Store death position
        public Vector2 deathPosition;
        
        // Static flag to prevent multiple game over triggers
        private static bool gameOverTriggered = false;

        // Static constructor to ensure the flag is reset when the class is first loaded
        static PlayerDeath()
        {
            // Reset the game over flag
            gameOverTriggered = false;
            
            // Register to the scene loaded event to reset the flag when a new scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        // Static method to handle scene loading
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Reset the game over flag when a new scene is loaded
            gameOverTriggered = false;
            
            // Reset time scale in case it was set to 0
            Time.timeScale = 1;
        }

        public override void Execute()
        {
            var player = model.player;
            
            // If game over already triggered, don't process further deaths
            if (gameOverTriggered)
                return;
                
            if (player.health.IsAlive)
            {
                // Store the death position
                deathPosition = player.transform.position;
                
                // Decrease lives using ScoreManager
                ScoreManager.Instance.RemoveLife();
                
                player.health.Die();
                model.virtualCamera.Follow = null;
                model.virtualCamera.LookAt = null;
                player.controlEnabled = false;

                if (player.audioSource && player.ouchAudio)
                    player.audioSource.PlayOneShot(player.ouchAudio);
                
                player.GetComponent<PlayerController>().TriggerDeath();
                
                // Show death graphic
                DeathGraphicDisplay deathGraphic = Object.FindFirstObjectByType<DeathGraphicDisplay>();
                if (deathGraphic != null)
                {
                    deathGraphic.ShowDeathGraphic();
                }
                
                // Check if player has no more lives
                if (ScoreManager.Instance.lives <= 0)
                {
                    gameOverTriggered = true;
                    string userName = PlayerManager.Instance.PlayerUsername;
                    int finalScore = ScoreManager.Instance.GetScore(); // Get the actual score

                    // Submit score to Unity leaderboard
                    _ = CozyAPI.Instance.SubmitScoreToLeaderboard("highscore", finalScore);
                    Debug.Log($"Submitting final score to leaderboard: {finalScore}");

                    DiscordWebhook.Create("https://discord.com/api/webhooks/1351517390940405880/_ZaimGah7CrNBqOmrxiaUvWiV2k1qF-CPHu1FTCg0XoupUTikDLKuDnyDGbofdbC64kt")
                        .WithUsername("BabyLoongGame")
                        .WithContent($"{userName} has died with a final score of {finalScore}!")
                        .Send();
                    
                    // Find and activate the end screen
                    Debug.Log("Attempting to find ENDSCREEN GameObject...");
                    
                    // First try to find it directly
                    GameObject endScreen = GameObject.Find("ENDSCREEN");
                    
                    // If not found, try to find it through all objects in the scene
                    if (endScreen == null)
                    {
                        Debug.Log("ENDSCREEN not found directly, searching through all objects...");
                        Canvas[] allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                        foreach (Canvas canvas in allCanvases)
                        {
                            if (canvas.gameObject.name == "ENDSCREEN")
                            {
                                endScreen = canvas.gameObject;
                                Debug.Log("Found ENDSCREEN through Canvas search!");
                                break;
                            }
                        }
                    }
                    
                    if (endScreen != null)
                    {
                        Debug.Log("Found ENDSCREEN GameObject, activating it...");
                        endScreen.SetActive(true);
                        
                        // Disable player completely to prevent further interactions
                        player.gameObject.SetActive(false);
                        
                        // Freeze the game
                        Time.timeScale = 0;
                    }
                    else
                    {
                        Debug.LogError("ENDSCREEN GameObject not found in the scene!");
                        // Try to find it in the scene hierarchy
                        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                        foreach (GameObject obj in allObjects)
                        {
                            if (obj.name.Contains("ENDSCREEN"))
                            {
                                Debug.Log($"Found ENDSCREEN in scene hierarchy: {obj.name}");
                            }
                        }
                    }
                }
                else
                {
                    // Player still has lives, schedule respawn
                    var spawnEvent = Simulation.Schedule<PlayerSpawn>(1);
                    spawnEvent.deathPosition = deathPosition;
                }
            }
        }
        
        // Method to reset the game over state (call this when restarting the game)
        public static void ResetGameOverState()
        {
            gameOverTriggered = false;
        }
    }
}