using System;
using UnityEngine;
using Unity.Services.Authentication;

namespace CozyFramework
{
    /// <summary>
    /// Receives Discord OAuth postMessage from the web popup and writes user id/username to Unity Cloud Save.
    /// Auto-created at runtime by CozyPickUsername as a "DiscordLoginReceiver" GameObject.
    /// </summary>
    public class DiscordLoginReceiver : MonoBehaviour
    {
        public static DiscordLoginReceiver Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Called via SendMessage from the web-side message listener.
        /// Expects JSON: { "type":"discord", "discord_id":"...", "username":"..." } or { "error":"..." }.
        /// </summary>
        public void OnDiscordUser(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var data = JsonUtility.FromJson<DiscordMessage>(json);
                if (!string.IsNullOrEmpty(data.error))
                {
                    Debug.LogWarning($"[Discord] Login error: {data.error}");
                    if (CozyPickUsername.Instance != null)
                        CozyPickUsername.Instance.OnDiscordLoginError(data.error);
                    return;
                }
                if (string.IsNullOrEmpty(data.discord_id) || string.IsNullOrEmpty(data.username))
                {
                    Debug.LogWarning("[Discord] Missing discord_id or username");
                    return;
                }
                ApplyDiscordUser(data.discord_id, data.username, data.token);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Discord] Parse error: {e.Message}");
            }
        }

        private async void ApplyDiscordUser(string discordId, string username, string sessionToken = null)
        {
            if (PlayerManager.Instance == null || CozyAPI.Instance == null) return;
            PlayerManager.Instance.PlayerDiscordId = discordId;
            PlayerManager.Instance.PlayerUsername = username;
            if (!string.IsNullOrEmpty(sessionToken))
                PlayerManager.Instance.PlayerSessionToken = sessionToken;
            PlayerManager.Instance.PlayerHasUsername = true;
            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(username);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Discord] UpdatePlayerNameAsync: {e.Message}");
            }
            await CozyAPI.Instance.SavePlayerData("username", username);
            await CozyAPI.Instance.SavePlayerData("discord_id", discordId);
            if (!string.IsNullOrEmpty(sessionToken))
                await CozyAPI.Instance.SavePlayerData("session_token", sessionToken);
            PlayerManager.Instance.UpdateUsernameDisplay();
            if (CozyPickUsername.Instance != null)
            {
                CozyPickUsername.Instance.HidePickUsernameView();
                CozyPickUsername.Instance.OnDiscordLoginSuccess();
            }
            Debug.Log($"[Discord] Linked: {username} ({discordId})");
        }

        [Serializable]
        private class DiscordMessage
        {
            public string type;
            public string discord_id;
            public string username;
            public string token;
            public string error;
        }
    }
}
