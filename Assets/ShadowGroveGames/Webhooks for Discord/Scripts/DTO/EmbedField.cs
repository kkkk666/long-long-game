using Newtonsoft.Json;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts.DTO
{
#nullable enable
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public struct EmbedField
    {
        /// <summary>
        /// Field name (Up to 256 characters)
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// Field value (Up to 1024 characters)
        /// </summary>
        [JsonProperty("value")]
        public string Value;

        /// <summary>
        /// Whether or not this field should display inline
        /// </summary>
        [JsonProperty("inline")]
        public bool Inline;

    }
#nullable disable
}
