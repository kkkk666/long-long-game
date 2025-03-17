using Newtonsoft.Json;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts.DTO
{
#nullable enable
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public struct EmbedUrl
    {
        /// <summary>
        /// Footer text
        /// </summary>
        [JsonProperty("url")]
        public string Url;

    }
#nullable disable
}
