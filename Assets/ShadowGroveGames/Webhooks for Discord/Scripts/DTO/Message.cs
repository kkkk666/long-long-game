using Newtonsoft.Json;
using System.Collections.Generic;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts.DTO
{
#nullable enable
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Message
    {
        /// <summary>
        /// Message contents (up to 2000 characters)
        /// </summary>
        [JsonProperty("content")]
        public string? Content = null;


        /// <summary>
        /// Username (up to 80 characters)
        /// </summary>
        [JsonProperty("username")]
        public string? Username = null;

        /// <summary>
        /// Url to an image used as an avatar
        /// </summary>
        [JsonProperty("avatar_url")]
        public string? Avatar = null;

        /// <summary>
        /// Thread name (Only works in forum channels, up to 80 characters)
        /// </summary>
        [JsonProperty("thread_name")]
        public string? ThreadName = null;

        /// <summary>
        /// Message flags
        /// </summary>
        [JsonProperty("flags")]
        public MessageFlags? Flags = null;

        /// <summary>
        /// A list of embeds maximal 10 per message!
        /// </summary>
        [JsonProperty("embeds")]
        public List<Embed> Embeds = new List<Embed>();

        /// <summary>
        /// A list of forum tag ids
        /// </summary>
        [JsonProperty("applied_tags")]
        public List<ulong> Tags = new List<ulong>();
    }
#nullable disable
}
