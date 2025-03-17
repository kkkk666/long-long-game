using UnityEngine;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts
{
    [CreateAssetMenu(fileName = "Data", menuName = "Shadow Grove Games/Discord Webhook Config")]
    public class DiscordWebhookConfigScriptableObject : ScriptableObject
    {
        [Header("General Config")]
        [Tooltip("The Webhook url to your channel")]
        [Min(1)]
        public string WebhookUrl;

        [Tooltip("Username that will be displayed above the message (Optional)")]
        public string Username;

        [Tooltip("A valid url to an image (Optional)")]
        public string AvatarUrl;

        [Header("Thread Config")]
        [Tooltip("A thread name can be used only for webhooks that refer to a forum channel (Optional)")]
        public string ThreadName;

        [Tooltip("The thread id must refer to an existing thread in a forum channel (Optional)")]
        public ulong ThreadId = 0;

        [Header("Flag Config")]
        [Tooltip("Do not include any embeds when serializing this message")]
        public bool SuppressEmbeds = false;

        [Tooltip("This message will not trigger push and desktop notifications")]
        public bool SuppressNotifications = false;
    }
}
