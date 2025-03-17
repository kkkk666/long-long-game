using Newtonsoft.Json;

#nullable enable
namespace ShadowGroveGames.WebhooksForDiscord.Scripts.DTO
{
    /// <summary>
    /// https://discord.com/developers/docs/resources/user#user-object
    /// </summary>
    public readonly struct MessageResponseUser
    {
        /// <summary>
        /// The user's id
        /// </summary>
        [JsonProperty("id")]
        public readonly ulong Id;

        /// <summary>
        /// The user's username, not unique across the platform
        /// </summary>
        [JsonProperty("username")]
        public readonly string Username;

        /// <summary>
        /// The user's 4-digit discord-tag
        /// </summary>
        [JsonProperty("discriminator")]
        public readonly string Discriminator;

        /// <summary>
        /// The user's avatar hash
        /// </summary>
        [JsonProperty("avatar")]
        public readonly string AvatarHash;

        /// <summary>
        /// Is a bot or webhook user
        /// </summary>
        [JsonProperty("bot")]
        public readonly bool IsBotUser;
    }
}
#nullable disable