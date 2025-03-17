using Newtonsoft.Json;
using ShadowGroveGames.WebhooksForDiscord.Scripts.Assets.ShadowGroveGames.Webhooks_for_Discord.Scripts.DTO;
using System;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts.DTO
{
#nullable enable
    public readonly struct MessageResponse
    {
        /// <summary>
        /// Id of the message
        /// </summary>
        [JsonProperty("id")]
        public readonly ulong Id;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("type")]
        public readonly string Type;

        /// <summary>
        /// The message contents (up to 2000 characters)
        /// </summary>
        [JsonProperty("content")]
        public readonly string? Content;

        /// <summary>
        /// Id of the channel the message was sent in
        /// </summary>
        [JsonProperty("channel_id")]
        public readonly ulong ChannelId;

        /// <summary>
        /// The author of this message
        /// </summary>
        [JsonProperty("author")]
        public readonly MessageResponseUser? Author;

        /// <summary>
        /// Array of attached files
        /// </summary>
        [JsonProperty("attachments")]
        public readonly MessageResponseAttachment[] Attachments;

        /// <summary>
        /// Array of embedded content
        /// </summary>
        [JsonProperty("embeds")]
        public readonly Embed[] Embeds;

        /// <summary>
        /// Users specifically mentioned in the message
        /// </summary>
        [JsonProperty("mentions")]
        public readonly MessageResponseUser[] UserMentions;

        /// <summary>
        /// Roles specifically mentioned in this message
        /// </summary>
        [JsonProperty("mention_roles")]
        public readonly ulong[] RoleMentions;

        /// <summary>
        /// Whether this message is pinned
        /// </summary>
        [JsonProperty("pinned")]
        public readonly bool Pinned;

        /// <summary>
        /// Whether this message mentions everyone
        /// </summary>
        [JsonProperty("mention_everyone")]
        public readonly bool MentionEveryone;

        /// <summary>
        /// Whether this was a TTS message
        /// </summary>
        [JsonProperty("tts")]
        public readonly bool IsTextToSpeech;

        /// <summary>
        /// When this message was sent
        /// </summary>
        [JsonProperty("timestamp")]
        public readonly DateTimeOffset? Timestamp;

        /// <summary>
        /// When this message was edited (or null if never)
        /// </summary>
        [JsonProperty("edited_timestamp")]
        public readonly DateTimeOffset? EditedTimestamp;

        /// <summary>
        /// Message flags
        /// </summary>
        [JsonProperty("flags")]
        public readonly MessageFlags? Flags;

        [JsonProperty("webhook_id")]
        public readonly ulong WebhookId;

        [JsonProperty("position")]
        public readonly ulong? MessagePosition;
    }
#nullable disable
}
