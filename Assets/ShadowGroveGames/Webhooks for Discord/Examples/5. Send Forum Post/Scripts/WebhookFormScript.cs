using ShadowGroveGames.WebhooksForDiscord.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowGroveGames.WebhooksForDiscord.Examples.SendForumPost
{
    public class WebhookFormScript : MonoBehaviour
    {
        [SerializeField]
        private InputField _webhookUrlInputField;

        [SerializeField]
        private InputField _postTitleInputField;

        [SerializeField]
        private InputField _messageInputField;

        public void SendWebhook()
        {
            string webhookUrl = _webhookUrlInputField?.text;
            string postTitle = _postTitleInputField?.text;
            string message = _messageInputField?.text;

            if (string.IsNullOrEmpty(webhookUrl) || string.IsNullOrEmpty(postTitle) || string.IsNullOrEmpty(message))
            {
                Debug.LogError("Please fill in all required fields!");
                return;
            }

            DiscordWebhook
                .Create(webhookUrl)
                .WithThreadName(postTitle)
                .WithContent(message)
                .Send();
        }

        public void ForumChannelFAQ()
        {
            Application.OpenURL("https://support.discord.com/hc/en-us/articles/6208479917079-Forum-Channels-FAQ");
        }
    }
}
