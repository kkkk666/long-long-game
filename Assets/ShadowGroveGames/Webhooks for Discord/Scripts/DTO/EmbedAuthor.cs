using Newtonsoft.Json;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts.DTO
{
#nullable enable
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public struct EmbedAuthor
    {
        /// <summary>
        /// Name of author
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// Url of author icon (only supports http(s) and attachments)
        /// </summary>
        [JsonProperty("icon_url")]
        public string? AvatarUrl;

        /// <summary>
        /// Url of author (only supports http(s))
        /// </summary>
        [JsonProperty("url")]
        public string? LinkUrl;

    }
#nullable disable
}
