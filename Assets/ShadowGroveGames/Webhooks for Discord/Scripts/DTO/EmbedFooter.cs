using Newtonsoft.Json;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts.DTO
{
#nullable enable
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public struct EmbedFooter
    {
        /// <summary>
        /// Footer text
        /// </summary>
        [JsonProperty("text")]
        public string Text;

        /// <summary>
        /// Url of footer icon (only supports http(s) and attachments)
        /// </summary>
        [JsonProperty("icon_url")]
        public string? IconUrl;

    }
#nullable disable
}
