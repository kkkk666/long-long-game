using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using ShadowGroveGames.WebhooksForDiscord.Scripts;
using CozyFramework; 
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Platformer.Gameplay
{
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        
        // Store death position
        public Vector2 deathPosition;
        
        // Store whether the player died in water
        public bool diedInWater = false;
        
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
                    Debug.Log("Attempting to find ENDSCREEN through CozyManager...");
                    
                    // Find CozyManager in the StartScreen scene
                    GameObject cozyManager = GameObject.Find("CozyManager");
                    if (cozyManager != null)
                    {
                        // Find the ENDSCREEN in CozyManager's children
                        Transform endScreenTransform = cozyManager.transform.Find("ENDSCREEN");
                        if (endScreenTransform != null)
                        {
                            Debug.Log("Found ENDSCREEN in CozyManager, activating it...");
                            endScreenTransform.gameObject.SetActive(true);
                            
                            // Disable player completely to prevent further interactions
                            player.gameObject.SetActive(false);
                            
                            // Freeze the game
                            Time.timeScale = 0;
                        }
                        else
                        {
                            Debug.LogError("ENDSCREEN not found in CozyManager!");
                        }
                    }
                    else
                    {
                        Debug.LogError("CozyManager not found in the scene!");
                    }
                }
                else
                {
                    // Player still has lives, schedule respawn
                    var spawnEvent = Simulation.Schedule<PlayerSpawn>(1);
                    spawnEvent.deathPosition = deathPosition;
                    spawnEvent.diedInWater = diedInWater;
                }
            }
        }
        
        // Method to reset the game over state (call this when restarting the game)
        public static void ResetGameOverState()
        {
            gameOverTriggered = false;
        }

        private Transform FindNearestSafeSpot()
        {
            // Get all safe spots in the scene
            SafeSpot[] safeSpots = Object.FindObjectsOfType<SafeSpot>();
            if (safeSpots.Length == 0)
            {
                Debug.LogWarning("No safe spots found in the scene!");
                return null;
            }

            // Get all death zones in the scene
            DeathZone[] deathZones = Object.FindObjectsOfType<DeathZone>();
            if (deathZones.Length == 0)
            {
                Debug.LogWarning("No death zones found in the scene!");
                return null;
            }

            // Get all water death zones in the scene
            DeathZone[] waterDeathZones = deathZones.Where(dz => dz.gameObject.layer == LayerMask.NameToLayer("Water")).ToArray();

            // Find the nearest safe spot that's not inside any death zone or water death zone
            Transform nearestSafeSpot = null;
            float nearestDistance = float.MaxValue;
            float maxDistance = diedInWater ? 20f : 10f; // Double the distance if died in water

            foreach (SafeSpot spot in safeSpots)
            {
                // Check if the safe spot is inside any death zone
                bool isInsideDeathZone = false;
                foreach (DeathZone deathZone in deathZones)
                {
                    if (deathZone.GetComponent<Collider2D>().bounds.Contains(spot.transform.position))
                    {
                        isInsideDeathZone = true;
                        break;
                    }
                }

                // Check if the safe spot is inside any water death zone
                bool isInsideWaterDeathZone = false;
                foreach (DeathZone waterDeathZone in waterDeathZones)
                {
                    if (waterDeathZone.GetComponent<Collider2D>().bounds.Contains(spot.transform.position))
                    {
                        isInsideWaterDeathZone = true;
                        break;
                    }
                }

                // Skip this safe spot if it's inside any death zone or water death zone
                if (isInsideDeathZone || isInsideWaterDeathZone)
                {
                    continue;
                }

                // Calculate distance to player's death position
                float distance = Vector2.Distance(spot.transform.position, deathPosition);
                
                // Skip if the distance is greater than our maximum allowed distance
                if (distance > maxDistance)
                {
                    continue;
                }

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSafeSpot = spot.transform;
                }
            }

            if (nearestSafeSpot == null)
            {
                // If no safe spot found, use SpawnPointFinder to find a safe position
                var player = model.player;
                Vector2 safePosition = SpawnPointFinder.FindSafeSpawnPoint(deathPosition, player.GetComponent<PlayerController>().groundLayer, 1.5f, diedInWater);
                
                // Create a temporary safe spot at the found position
                GameObject tempSafeSpot = new GameObject("TempSafeSpot");
                tempSafeSpot.transform.position = safePosition;
                tempSafeSpot.AddComponent<SafeSpot>();
                return tempSafeSpot.transform;
            }

            return nearestSafeSpot;
        }
    }
}