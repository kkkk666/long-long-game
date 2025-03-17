using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts.DTO
{
#nullable enable
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Embed
    {
        /// <summary>
        /// Title of embed
        /// </summary>
        [JsonProperty("title")]
        public string? Title = null;

        /// <summary>
        /// Value of embed
        /// </summary>
        [JsonProperty("description")]
        public string? Description = null;

        /// <summary>
        /// Value of embed
        /// </summary>
        [JsonProperty("url")]
        public string? Url = null;

        /// <summary>
        /// Timestamp of embed content
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime? Timestamp = null;

        /// <summary>
        /// Embed accent color
        /// </summary>
        [JsonProperty("color"), JsonConverter(typeof(IntegerToColorConverter))]
        public Color? Color = null;

        /// <summary>
        /// Footer information
        /// </summary>
        [JsonProperty("footer")]
        public EmbedFooter? Footer = null;

        /// <summary>
        /// Image information
        /// </summary>
        [JsonProperty("image")]
        public EmbedUrl? Image = null;

        /// <summary>
        /// Thumbnail information
        /// </summary>
        [JsonProperty("thumbnail")]
        public EmbedUrl? Thumbnail = null;

        /// <summary>
        /// Author information
        /// </summary>
        [JsonProperty("author")]
        public EmbedAuthor? Author = null;

        /// <summary>
        /// List of fields
        /// </summary>
        [JsonProperty("fields")]
        public List<EmbedField> Fields = new List<EmbedField>();

    }
#nullable disable
}
