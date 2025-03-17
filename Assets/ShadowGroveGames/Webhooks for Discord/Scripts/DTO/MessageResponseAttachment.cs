using Newtonsoft.Json;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts.Assets.ShadowGroveGames.Webhooks_for_Discord.Scripts.DTO
{
#nullable enable
    public readonly struct MessageResponseAttachment
    {
        /// <summary>
        /// Unique identifier for the attachment.
        /// </summary>
        [JsonProperty("id")]
        public readonly string Id;

        /// <summary>
        /// Name of the file.
        /// </summary>
        [JsonProperty("filename")]
        public readonly string Filename;

        /// <summary>
        /// Description for the file.
        /// </summary>
        [JsonProperty("description")]
        public readonly string? Description;

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        [JsonProperty("size")]
        public readonly int Size;

        /// <summary>
        /// URL of the attachment.
        /// </summary>
        [JsonProperty("url")]
        public readonly string Url;

        /// <summary>
        /// Proxied URL of the attachment.
        /// </summary>
        [JsonProperty("proxy_url")]
        public readonly string ProxyUrl;

        /// <summary>
        /// Width of the attachment (if it's an image).
        /// </summary>
        [JsonProperty("width")]
        public readonly int? Width;

        /// <summary>
        /// Height of the attachment (if it's an image).
        /// </summary>
        [JsonProperty("height")]
        public readonly int? Height;

        /// <summary>
        /// MIME type of the file.
        /// </summary>
        [JsonProperty("content_type")]
        public readonly string? ContentType;

        /// <summary>
        /// Whether this attachment is ephemeral
        /// </summary>
        [JsonProperty("ephemeral")]
        public readonly bool Ephemeral;
    }
#nullable disable
}
