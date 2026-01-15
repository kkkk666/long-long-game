using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using CozyFramework; 
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using TMPro;
using UnityEngine.Networking;

namespace Platformer.Gameplay
{
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        // API key for Vercel webhook authentication
        // IMPORTANT: Change this when you deploy to a new Vercel instance
        private const string WEBHOOK_API_KEY = "BabyDragon2024SecureKey";

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
                
                // Check if player died in water
                Collider2D[] colliders = Physics2D.OverlapPointAll(deathPosition);
                foreach (var collider in colliders)
                {
                    if (collider.gameObject.layer == LayerMask.NameToLayer("Water"))
                    {
                        diedInWater = true;
                        break;
                    }
                }
                
                // Spawn water splash effect if died in water
                if (diedInWater && player.GetComponent<PlayerController>().waterSplashEffectPrefab != null)
                {
                    // Get the player controller and offset
                    var playerController = player.GetComponent<PlayerController>();
                    Vector3 spawnPosition = deathPosition + playerController.waterSplashOffset;
                    
                    // Spawn the effect at the adjusted position
                    GameObject splashEffect = UnityEngine.Object.Instantiate(playerController.waterSplashEffectPrefab, spawnPosition, Quaternion.identity);
                    
                    // Get the particle system component
                    ParticleSystem particles = splashEffect.GetComponent<ParticleSystem>();
                    if (particles != null)
                    {
                        // Play the particle system
                        particles.Play();
                        
                        // Destroy the game object after the particle system has finished
                        float duration = particles.main.duration + particles.main.startLifetime.constantMax;
                        UnityEngine.Object.Destroy(splashEffect, duration);
                    }
                    else
                    {
                        // If no particle system found, destroy after a default time
                        UnityEngine.Object.Destroy(splashEffect, 2f);
                    }
                }
                
                // Reset the diedInWater flag after processing the death
                diedInWater = false;
                
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

                    // Submit score to Unity leaderboard and check if it's a top score
                    HandleTopScoreAndDiscordMessage(userName, finalScore);

                    // Find and activate the end screen
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log("Attempting to find ENDSCREEN through CozyManager...");
                    #endif

                    // Find CozyManager in the StartScreen scene
                    GameObject cozyManager = GameObject.Find("CozyManager");
                    if (cozyManager != null)
                    {
                        // Find the ENDSCREEN in CozyManager's children
                        Transform endScreenTransform = cozyManager.transform.Find("ENDSCREEN");
                        if (endScreenTransform != null)
                        {
                            #if UNITY_EDITOR || DEVELOPMENT_BUILD
                            Debug.Log("Found ENDSCREEN in CozyManager, activating it...");
                            #endif
                            endScreenTransform.gameObject.SetActive(true);
                            
                            // Disable player completely to prevent further interactions
                            player.gameObject.SetActive(false);
                            
                            // Freeze the game
                            Time.timeScale = 0;
                        }
                        else
                        {
                            #if UNITY_EDITOR || DEVELOPMENT_BUILD
                            Debug.LogError("ENDSCREEN not found in CozyManager!");
                            #endif
                        }
                    }
                    else
                    {
                        #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogError("CozyManager not found in the scene!");
                        #endif
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
        
        private async void HandleTopScoreAndDiscordMessage(string userName, int finalScore)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Player died with a final score of {finalScore}");
            #endif

            try
            {
                // First query the leaderboard to get the current top score BEFORE submitting the new score
                var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync("highscore");
                int currentTopScore = 0;
                bool isNewHighScore = false;
                
                // Check if there are existing scores
                if (scoresResponse != null && scoresResponse.Results.Count > 0)
                {
                    var topEntry = scoresResponse.Results[0];
                    currentTopScore = (int)topEntry.Score;
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"Current top score on leaderboard before submission: {currentTopScore}");
                    #endif

                    isNewHighScore = finalScore > currentTopScore;
                }
                else
                {
                    isNewHighScore = true;
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log("No existing scores found. This is the first high score.");
                    #endif
                }
                
                // Now submit the score to the leaderboard
                await CozyAPI.Instance.SubmitScoreToLeaderboard("highscore", finalScore);
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"Submitted final score to leaderboard: {finalScore}");
                #endif

                // Find and update the high score text if this is a new high score
                GameObject cozyManager = GameObject.Find("CozyManager");
                if (cozyManager != null)
                {
                    Transform endScreenTransform = cozyManager.transform.Find("ENDSCREEN");
                    if (endScreenTransform != null)
                    {
                        // Updated path to include the Main object
                        Transform mainTransform = endScreenTransform.Find("Main");
                        if (mainTransform != null)
                        {
                            TextMeshProUGUI highScoreText = mainTransform.Find("HighScoreText")?.GetComponent<TextMeshProUGUI>();
                            if (highScoreText != null)
                            {
                                // Only show the high score message if it's a new high score
                                if (isNewHighScore)
                                {
                                    highScoreText.text = $"Congrats - New High Score of - {finalScore} !!!";
                                    highScoreText.gameObject.SetActive(true);
                                }
                                else
                                {
                                    highScoreText.gameObject.SetActive(false);
                                }
                            }
                            else
                            {
                                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                                Debug.LogError("HighScoreText component not found in Main object!");
                                #endif
                            }
                        }
                        else
                        {
                            #if UNITY_EDITOR || DEVELOPMENT_BUILD
                            Debug.LogError("Main object not found in ENDSCREEN!");
                            #endif
                        }
                    }
                }

                // Send score to Vercel endpoint
                string vercelEndpoint = "https://babydragongame-webhook.vercel.app/api/discord-webhook";
                WWWForm form = new WWWForm();
                form.AddField("userName", userName);
                form.AddField("score", finalScore.ToString());
                form.AddField("isNewHighScore", isNewHighScore.ToString());
                form.AddField("currentTopScore", currentTopScore.ToString());
                form.AddField("apiKey", WEBHOOK_API_KEY);

                // Only send to Discord if it's a new high score
                if (isNewHighScore)
                {
                    using (UnityWebRequest www = UnityWebRequest.Post(vercelEndpoint, form))
                    {
                        await www.SendWebRequest();

                        #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            Debug.Log("New high score successfully sent to Discord");
                        }
                        else
                        {
                            Debug.LogError($"Error sending score to Discord: {www.error}");
                        }
                        #endif
                    }
                }
                else
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log("Not a new high score, skipping Discord message");
                    #endif
                }
            }
            catch (System.Exception ex)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"Error handling leaderboard: {ex.Message}");
                #endif

                // Try to submit the score anyway if there was an error
                try
                {
                    await CozyAPI.Instance.SubmitScoreToLeaderboard("highscore", finalScore);
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"Submitted final score to leaderboard after error: {finalScore}");
                    #endif
                }
                catch (System.Exception submitEx)
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError($"Error submitting score: {submitEx.Message}");
                    #endif
                }
                
                // Try to send score to Vercel endpoint even if there was an error
                try
                {
                    string vercelEndpoint = "https://babydragongame-webhook.vercel.app/api/discord-webhook";
                    WWWForm form = new WWWForm();
                    form.AddField("userName", userName);
                    form.AddField("score", finalScore.ToString());
                    form.AddField("isNewHighScore", "false");
                    form.AddField("currentTopScore", "0");
                    form.AddField("apiKey", WEBHOOK_API_KEY);

                    using (UnityWebRequest www = UnityWebRequest.Post(vercelEndpoint, form))
                    {
                        await www.SendWebRequest();
                        #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            Debug.Log("Score successfully sent to server after error");
                        }
                        else
                        {
                            Debug.LogError($"Error sending score to server after error: {www.error}");
                        }
                        #endif
                    }
                }
                catch (System.Exception webEx)
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError($"Error sending to Vercel endpoint: {webEx.Message}");
                    #endif
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
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("No safe spots found in the scene!");
                #endif
                return null;
            }

            // Get all death zones in the scene
            DeathZone[] deathZones = Object.FindObjectsOfType<DeathZone>();
            if (deathZones.Length == 0)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("No death zones found in the scene!");
                #endif
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