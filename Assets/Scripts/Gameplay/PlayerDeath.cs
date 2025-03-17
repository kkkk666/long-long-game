using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

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
                model.virtualCamera.Follow = null;
                model.virtualCamera.LookAt = null;
                player.controlEnabled = false;

                if (player.audioSource && player.ouchAudio)
                    player.audioSource.PlayOneShot(player.ouchAudio);
                
                player.GetComponent<PlayerController>().TriggerDeath();
                
                // Check if player has no more lives
                if (ScoreManager.Instance.lives <= 0)
                {
                    // Set the game over flag to prevent further deaths
                    gameOverTriggered = true;
                    
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
                        
                        // Make sure we have a HighScoreManager
                        if (HighScoreManager.Instance == null)
                        {
                            GameObject highScoreManagerObj = new GameObject("HighScoreManager");
                            highScoreManagerObj.AddComponent<HighScoreManager>();
                        }
                        
                        // Get or add the HighScoreUI component
                        HighScoreUI highScoreUI = endScreen.GetComponent<HighScoreUI>();
                        if (highScoreUI == null)
                        {
                            highScoreUI = endScreen.AddComponent<HighScoreUI>();
                        }
                        
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