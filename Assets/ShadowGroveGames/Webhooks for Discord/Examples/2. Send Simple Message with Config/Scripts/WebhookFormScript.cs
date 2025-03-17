using ShadowGroveGames.WebhooksForDiscord.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowGroveGames.WebhooksForDiscord.Examples.SendSimpleMessageWithConfig
{
    public class WebhookFormScript : MonoBehaviour
    {
        [SerializeField]
        private InputField _messageInputField;

        [SerializeField]
        private DiscordWebhookConfigScriptableObject _config;

        public void SendWebhook()
        {
            string message = _messageInputField?.text;

            if (_config.WebhookUrl == "MY WEBHOOK URL")
            {
                Debug.LogError("Add your configuration into the \"MyDiscordWebhookConfig\" file!");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                Debug.LogError("Please fill in all required fields!");
                return;
            }

            DiscordWebhook
                .Create(_config)
                .WithContent(message)
                .Send();
        }
    }
}
