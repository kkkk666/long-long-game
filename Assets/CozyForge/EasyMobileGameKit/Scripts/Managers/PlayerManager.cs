using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.CloudSave.Models;
using UnityEngine;
using TMPro;

namespace CozyFramework
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("PLAYER DATA")]
        public bool PlayerHasUsername;
        public string PlayerUsername;
        public string PlayerDiscordId;
        /// <summary>JWT from Discord login; used to submit score to /api/submit-score (never send API key from client).</summary>
        public string PlayerSessionToken;
        public int PlayerHighScore;
        public int PlayerTotalMatches;

        public static PlayerManager Instance;

        private void Awake()
        {
            SingletonHelper.InitializeSingleton(ref Instance, this);
        }

        private void Start()
        {
            CozyEvents.AuthSuccess += InitializeGame;
        }

        private void OnDisable()
        {
            CozyEvents.AuthSuccess -= InitializeGame;
        }


        private void InitializeGame()
        {
            LoadPlayerData();
        }


        private async void LoadPlayerData()
        {
            var keysToLoad = new HashSet<string>
            {
                "highscore",
                "matches",
                "username",
                "discord_id",
                "session_token"
            };

            var loadedData = await CozyCloudSave.LoadData(keysToLoad);
            if (loadedData == null)
            {
                Debug.LogWarning("LoadedData was null.");
                return;
            }

            if (loadedData.TryGetValue("highscore", out var highScoreItem))
            {
                var highScore = ConvertItem<int>(highScoreItem);
                if (!EqualityComparer<int>.Default.Equals(highScore, default(int)))
                {
                    PlayerHighScore = highScore;
                }
            }

            if (loadedData.TryGetValue("matches", out var totalMatchesItem))
            {
                var totalMatches = ConvertItem<int>(totalMatchesItem);
                if (!EqualityComparer<int>.Default.Equals(totalMatches, default(int)))
                {
                    PlayerTotalMatches = totalMatches;
                }
            }

            if (loadedData.TryGetValue("username", out var usernameItem))
            {
                string username = ConvertItem<string>(usernameItem);
                if (!string.IsNullOrEmpty(username))
                {
                    PlayerHasUsername = true;
                    PlayerUsername = username;
                }
            }
            if (loadedData.TryGetValue("discord_id", out var discordIdItem))
            {
                string id = ConvertItem<string>(discordIdItem);
                if (!string.IsNullOrEmpty(id))
                    PlayerDiscordId = id;
            }
            if (loadedData.TryGetValue("session_token", out var tokenItem))
            {
                string tok = ConvertItem<string>(tokenItem);
                if (!string.IsNullOrEmpty(tok))
                    PlayerSessionToken = tok;
            }

            if (PlayerHasUsername)
            {
                try
                {
                    PlayerUsername = await AuthenticationService.Instance.GetPlayerNameAsync();
                }
                catch
                {
                    if (string.IsNullOrEmpty(PlayerUsername) && loadedData.TryGetValue("username", out var u))
                        PlayerUsername = ConvertItem<string>(u);
                }
                Debug.Log($"Loaded username: {PlayerUsername}");
                UpdateUsernameDisplay();
            }
        }

        private T ConvertItem<T>(Item item)
        {
            try
            {
                string jsonString = item.Value.GetAsString();
                
                if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(jsonString, typeof(T));
                }
                else
                {
                    return JsonUtility.FromJson<T>(jsonString);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to convert value to type {typeof(T)}: {e.Message}");
                return default;
            }
        }

        public void UpdateUsernameDisplay()
        {
            // Find and update the USERNAME text in the current scene
            GameObject usernameObj = GameObject.Find("USERNAME");
            if (usernameObj != null)
            {
                var usernameText = usernameObj.GetComponent<TMP_Text>();
                if (usernameText != null && PlayerHasUsername)
                {
                    usernameText.text = PlayerUsername;
                    Debug.Log($"Updated USERNAME display to: {PlayerUsername}");
                }
            }
        }

    }
}
