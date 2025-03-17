using ShadowGroveGames.WebhooksForDiscord.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowGroveGames.WebhooksForDiscord.Examples.SendSimpleMessage
{
    public class WebhookFormScript : MonoBehaviour
    {
        [SerializeField]
        private InputField _webhookUrlInputField;

        [SerializeField]
        private InputField _usernameInputField;

        [SerializeField]
        private InputField _avatarUrlInputField;

        [SerializeField]
        private InputField _messageInputField;

        public void SendWebhook()
        {
            string webhookUrl = _webhookUrlInputField?.text;
            string username = _usernameInputField?.text;
            string avatarUrl = _avatarUrlInputField?.text;
            string message = _messageInputField?.text;

            if (string.IsNullOrEmpty(webhookUrl) || string.IsNullOrEmpty(message))
            {
                Debug.LogError("Please fill in all required fields!");
                return;
            }

            DiscordWebhook
                .Create(webhookUrl)
                .WithUsername(username)
                .WithAvatar(avatarUrl)
                .WithContent(message)
                .Send();
        }

        public void HowToWebhook()
        {
            Application.OpenURL("https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks");
        }
    }
}
