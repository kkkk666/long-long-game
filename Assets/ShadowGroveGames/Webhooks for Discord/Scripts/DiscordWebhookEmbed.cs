using ShadowGroveGames.WebhooksForDiscord.Scripts.DTO;
using System;
using UnityEngine;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts
{
#nullable enable
    public class DiscordWebhookEmbed
    {

        private Embed _embed;

        private DiscordWebhookEmbed()
        {
            _embed = new Embed();
        }

        public static DiscordWebhookEmbed Create()
        {
            return new DiscordWebhookEmbed();
        }

        /// <summary>
        /// Add a title with a url to the embed
        /// </summary>
        /// <param name="title">Title text</param>
        /// <param name="url">Title link</param>
        /// <returns></returns>
        public DiscordWebhookEmbed WithTitle(string title, string? url = null)
        {
            if (string.IsNullOrEmpty(title))
            {
                _embed.Title = null;
                return this;
            }

            if (string.IsNullOrEmpty(url))
                url = null;

            _embed.Title = title;
            _embed.Url = url;

            return this;
        }

        /// <summary>
        /// Add a value to the embed
        /// </summary>
        public DiscordWebhookEmbed WithDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                _embed.Description = null;
                return this;
            }

            _embed.Description = description;

            return this;
        }

        /// <summary>
        /// Add a timestamp to the embed
        /// </summary>
        public DiscordWebhookEmbed WithTimestamp(DateTime timestamp)
        {
            if (timestamp == null)
            {
                _embed.Timestamp = null;
                return this;
            }

            _embed.Timestamp = timestamp;

            return this;
        }

        /// <summary>
        /// Add a accent color to the embed
        /// </summary>
        public DiscordWebhookEmbed WithColor(Color? color)
        {
            if (color == null)
            {
                _embed.Color = null;
                return this;
            }

            _embed.Color = color;

            return this;
        }

        /// <summary>
        /// Add author information to the embed
        /// </summary>
        /// <param name="name">Name of author</param>
        /// <param name="avatarUrl">Url of author icon (only supports http(s) and attachments)</param>
        /// <param name="linkUrl">Url of author (only supports http(s))</param>
        public DiscordWebhookEmbed WithAuthor(string name, string? avatarUrl = null, string? linkUrl = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                _embed.Author = null;
                return this;
            }

            if (string.IsNullOrEmpty(avatarUrl))
                avatarUrl = null;

            if (string.IsNullOrEmpty(linkUrl))
                linkUrl = null;

            _embed.Author = new EmbedAuthor() { Name = name, AvatarUrl = avatarUrl, LinkUrl = linkUrl };
            return this;
        }

        /// <summary>
        /// Add footer information to the embed
        /// </summary>
        /// <param name="text">Footer text</param>
        /// <param name="iconUrl">Url of footer icon (only supports http(s) and attachments)</param>
        public DiscordWebhookEmbed WithFooter(string text, string? iconUrl = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                _embed.Author = null;
                return this;
            }

            if (string.IsNullOrEmpty(iconUrl))
                iconUrl = null;

            _embed.Footer = new EmbedFooter() { Text = text, IconUrl = iconUrl };
            return this;
        }

        /// <summary>
        /// Add a image to the embed
        /// </summary>
        public DiscordWebhookEmbed WithImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                _embed.Image = null;
                return this;
            }

            _embed.Image = new EmbedUrl() { Url = imageUrl };

            return this;
        }

        /// <summary>
        /// Add a thumbnail to the embed
        /// </summary>
        public DiscordWebhookEmbed WithThumbnail(string thumbnailUrl)
        {
            if (string.IsNullOrEmpty(thumbnailUrl))
            {
                _embed.Thumbnail = null;
                return this;
            }

            _embed.Thumbnail = new EmbedUrl() { Url = thumbnailUrl };

            return this;
        }

        /// <summary>
        /// Add a field to the embed
        /// </summary>
        public DiscordWebhookEmbed AddField(string name, string value, bool inline = false)
        {
            if (_embed.Fields.Count >= 25 || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                return this;

            _embed.Fields.Add(new EmbedField() { Name = name, Value = value, Inline = inline });

            return this;
        }

        /// <summary>
        /// Add a spacer between fields inside a embed
        /// </summary>
        public DiscordWebhookEmbed AddFieldSpacer()
        {
            _embed.Fields.Add(new EmbedField() { Name = '\u200b'.ToString(), Value = '\u200b'.ToString() });

            return this;
        }

        public Embed Build()
        {
            return _embed;
        }
    }
#nullable disable
}
