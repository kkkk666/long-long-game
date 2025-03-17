using ShadowGroveGames.WebhooksForDiscord.Scripts;
using ShadowGroveGames.WebhooksForDiscord.Scripts.DTO;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowGroveGames.WebhooksForDiscord.Examples.SendEmbedMessage
{
    public class WebhookFormScript : MonoBehaviour
    {
        [SerializeField]
        private InputField _webhookUrlInputField;

        [Header("Title")]
        [SerializeField]
        private InputField _titleInputField;

        [SerializeField]
        private InputField _titleLinkInputField;

        [Header("Author")]
        [SerializeField]
        private InputField _authorInputField;

        [SerializeField]
        private InputField _authorAvatarUrlInputField;

        [Header("Description")]
        [SerializeField]
        private InputField _descriptionInputField;

        [Space]

        [Header("Field 1")]
        [SerializeField]
        private InputField _fieldName1InputField;
        [SerializeField]
        private InputField _fieldValue1InputField;

        [Header("Field 2")]
        [SerializeField]
        private InputField _fieldName2InputField;
        [SerializeField]
        private InputField _fieldValue2InputField;

        [Header("Field 3")]
        [SerializeField]
        private InputField _fieldName3InputField;
        [SerializeField]
        private InputField _fieldValue3InputField;

        public void SendWebhook()
        {
            string webhookUrl = _webhookUrlInputField?.text;

            string title = _titleInputField?.text;
            string titleLink = _titleLinkInputField?.text;

            string author = _authorInputField?.text;
            string authorAvatarUrl = _authorAvatarUrlInputField?.text;

            string description = _descriptionInputField?.text;

            string fieldName1 = _fieldName1InputField?.text;
            string fieldValue1 = _fieldValue1InputField?.text;

            string fieldName2 = _fieldName2InputField?.text;
            string fieldValue2 = _fieldValue2InputField?.text;

            string fieldName3 = _fieldName3InputField?.text;
            string fieldValue3 = _fieldValue3InputField?.text;

            if (string.IsNullOrEmpty(webhookUrl) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(description))
            {
                Debug.LogError("Please fill in all required fields!");
                return;
            }

            DiscordWebhook
                .Create(webhookUrl)
                .AddEmbed(
                    DiscordWebhookEmbed
                    .Create()
                    .WithTitle(title, titleLink)
                    .WithAuthor(author, authorAvatarUrl)
                    .WithDescription(description)
                    .AddField(fieldName1, fieldValue1)
                    .AddField(fieldName2, fieldValue2, true)
                    .AddField(fieldName3, fieldValue3, true)
                    .Build()
                )
                .Send();
        }

        public void HowToWebhook()
        {
            Application.OpenURL("https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks");
        }
    }
}
