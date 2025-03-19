using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
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
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        // Static method to handle scene loading
        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
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
         
                player.controlEnabled = false;

                if (player.audioSource && player.ouchAudio)
                    player.audioSource.PlayOneShot(player.ouchAudio);
                
                player.GetComponent<PlayerController>().TriggerDeath();
                
                // Check if player has no more lives
                if (ScoreManager.Instance.lives <= 0)
                {
                    // Set the game over flag to prevent further deaths
                    gameOverTriggered = true;
                    string userName = PlayerManager.Instance.PlayerUsername;
                    int finalScore = ScoreManager.Instance.GetScore(); // Get the actual score

                    // Submit score to Unity leaderboard
                    _ = CozyAPI.Instance.SubmitScoreToLeaderboard("HIGHEST_SCORE", finalScore);
                    Debug.Log($"Submitting final score to leaderboard: {finalScore}");

                    DiscordWebhook.Create("https://discord.com/api/webhooks/1351517390940405880/_ZaimGah7CrNBqOmrxiaUvWiV2k1qF-CPHu1FTCg0XoupUTikDLKuDnyDGbofdbC64kt")
                        .WithUsername("BabyLoongGame")
                        .WithContent($"{userName} has died with a final score of {finalScore}!")
                        .Send();
                    // Find all Canvas objects including inactive ones
                    Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
                    GameObject endScreen = null;
                    
                    foreach (Canvas canvas in allCanvases)
                    {
                        if (canvas.name == "ENDSCREEN")
                        {
                            endScreen = canvas.gameObject;
                            break;
                        }
                    }
                    
                    if (endScreen != null)
                    {
                        endScreen.SetActive(true);
                        
                   
                        
                        // Disable player completely to prevent further interactions
                        player.gameObject.SetActive(false);
                        
                        // Freeze the game
                        Time.timeScale = 0;
                    }
                    else
                    {
                        Debug.LogError("ENDSCREEN Canvas not found! Please check the exact name in the hierarchy.");
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