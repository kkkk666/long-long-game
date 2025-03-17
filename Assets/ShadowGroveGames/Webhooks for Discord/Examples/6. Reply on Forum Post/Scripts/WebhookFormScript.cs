using ShadowGroveGames.WebhooksForDiscord.Scripts;
using ShadowGroveGames.WebhooksForDiscord.Scripts.DTO;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowGroveGames.WebhooksForDiscord.Examples.ReplyOnForumPost
{
    public class WebhookFormScript : MonoBehaviour
    {
        [SerializeField]
        private InputField _webhookUrlInputField;

        [SerializeField]
        private InputField _postTitleInputField;

        [SerializeField]
        private InputField _postIdInputField;

        [SerializeField]
        private InputField _messageInputField;

        public void SendWebhook()
        {
            StartCoroutine(SendWebhookAsync());
        }

        private IEnumerator SendWebhookAsync()
        {
            string webhookUrl = _webhookUrlInputField?.text;
            string postTitle = _postTitleInputField?.text;
            string postId = _postIdInputField?.text;
            string message = _messageInputField?.text;

            if (string.IsNullOrEmpty(webhookUrl) || (string.IsNullOrEmpty(postTitle) && string.IsNullOrEmpty(postId)) || string.IsNullOrEmpty(message))
            {
                Debug.LogError("Please fill in all required fields and enter a post title OR post id!");
                yield break;
            }

            var discordWebhook = DiscordWebhook
               .Create(webhookUrl)
               .WithThreadName(postTitle)
               .WithContent(message);

            // We use the 
            if (!string.IsNullOrEmpty(postId))
                discordWebhook.WithThreadId(ulong.Parse(postId));

            Task<MessageResponse?> messageResponse = discordWebhook.SendAsync();
            yield return new WaitUntil(() => messageResponse.IsFaulted || messageResponse.IsCanceled || messageResponse.IsCompleted);

            // Only the first message id of a post can be used to send additional messages into the same thread
            if (string.IsNullOrEmpty(postId))
            {
                _postTitleInputField.text = "";
                _postIdInputField.text = messageResponse.Result.Value.Id.ToString();
            }
        }

        public void ForumChannelFAQ()
        {
            Application.OpenURL("https://support.discord.com/hc/en-us/articles/6208479917079-Forum-Channels-FAQ");
        }
    }
}
