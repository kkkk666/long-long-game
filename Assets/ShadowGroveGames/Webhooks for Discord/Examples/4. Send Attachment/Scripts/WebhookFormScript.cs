using ShadowGroveGames.WebhooksForDiscord.Scripts;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowGroveGames.WebhooksForDiscord.Examples.SendAttachment
{
    public class WebhookFormScript : MonoBehaviour
    {
        [SerializeField]
        private InputField _webhookUrlInputField;

        [SerializeField]
        private InputField _usernameInputField;

        [SerializeField]
        private InputField _messageInputField;

        [SerializeField]
        private InputField _filePathInputField;

        public void SendWebhook()
        {
            string webhookUrl = _webhookUrlInputField?.text;
            string username = _usernameInputField?.text;
            string message = _messageInputField?.text;
            string filePath = _filePathInputField?.text;

            if (string.IsNullOrEmpty(webhookUrl) || string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("Please fill in all required fields!");
                return;
            }

            if (!File.Exists(filePath))
            {
                Debug.LogError($"File \"{filePath}\" not exists!");
                return;
            }

            DiscordWebhook
                .Create(webhookUrl)
                .WithUsername(username)
                .WithContent(message)
                .AddAttachment(filePath)
                .Send();
        }

        public void HowToWebhook()
        {
            Application.OpenURL("https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks");
        }
    }
}
