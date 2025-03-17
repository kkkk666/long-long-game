using Newtonsoft.Json;
using ShadowGroveGames.WebhooksForDiscord.Scripts.DTO;
using ShadowGroveGames.WebhooksForDiscord.Scripts.Extentions;
using ShadowGroveGames.WebhooksForDiscord.Scripts.Helper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts
{
#nullable enable
    public class DiscordWebhook
    {
        private const long ATTACHMENT_DISCORD_MAX_SIZE_IN_BYTES = (long)25e+6; // 25 MB

        /// <summary>
        /// Webhook url
        /// </summary>
        private string _webhookUrl;

        /// <summary>
        /// The thread ID must be specified via the query parameters and point to an existing thread
        /// </summary>
        private ulong? _threadId = null;

        /// <summary>
        /// The timeout for the webhook send in secounds
        /// </summary>
        private int _sendTimeout = 60;

        /// <summary>
        /// Message information
        /// </summary>
        private Message _message;

        private List<Attachment> _attachments = new List<Attachment>();

        private DiscordWebhook(string webhookUrl)
        {
            _webhookUrl = webhookUrl;
            _message = new Message();
        }

        private DiscordWebhook(DiscordWebhookConfigScriptableObject config)
        {
            _webhookUrl = config.WebhookUrl;

            if (config.ThreadId > 0)
                _threadId = config.ThreadId;

            _message = new Message();

            WithUsername(config.Username);
            WithAvatar(config.AvatarUrl);
            WithThreadName(config.ThreadName);
            SuppressEmbeds(config.SuppressEmbeds);
            SuppressNotifications(config.SuppressNotifications);
        }

        public static DiscordWebhook Create(string webhookUrl)
        {
            return new DiscordWebhook(webhookUrl);
        }

        public static DiscordWebhook Create(DiscordWebhookConfigScriptableObject config)
        {
            return new DiscordWebhook(config);
        }

        /// <summary>
        /// Add message contant
        /// </summary>
        /// <param name="content">The message can contain up to 2000 characters</param>
        public DiscordWebhook WithContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                _message.Content = null;
                return this;
            }

            _message.Content = content;
            return this;
        }

        /// <summary>
        /// Add a username that will be displayed above the message
        /// </summary>
        /// <param name="username">The username can be 80 characters long</param>
        public DiscordWebhook WithUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                _message.Username = null;
                return this;
            }

            _message.Username = username;
            return this;
        }

        /// <summary>
        /// Add a avatar url
        /// </summary>
        /// <param name="avatarUrl">A valid url to an image</param>
        public DiscordWebhook WithAvatar(string avatarUrl)
        {
            if (string.IsNullOrEmpty(avatarUrl))
            {
                _message.Avatar = null;
                return this;
            }

            _message.Avatar = avatarUrl;
            return this;
        }

        /// <summary>
        /// A thread name can be used only for webhooks that refer to a forum channel
        /// </summary>
        /// <param name="threadName">The thread name can be 80 characters long</param>
        public DiscordWebhook WithThreadName(string threadName)
        {
            if (string.IsNullOrEmpty(threadName))
            {
                _message.ThreadName = null;
                return this;
            }

            // A message cannot have a thread ID and a thread name at the same time!
            if (_threadId > 0)
                _threadId = 0;

            _message.ThreadName = threadName;
            return this;
        }

        /// <summary>
        /// The thread id must refer to an existing thread in a forum channel
        /// </summary>
        /// <param name="threadId">Activate the developer mode in the advanced Discord settings and right click on a thread to copy the thread id</param>
        public DiscordWebhook WithThreadId(ulong threadId)
        {
            if (threadId <= 0)
            {
                _threadId = null;
                return this;
            }

            // A message cannot have a thread id and a thread name at the same time!
            if (!string.IsNullOrEmpty(_message.ThreadName))
                _message.ThreadName = null;

            _threadId = threadId;
            return this;
        }

        public DiscordWebhook AddEmbed(Embed embed)
        {
            if (_message.Embeds.Count >= 10)
                return this;

            _message.Embeds.Add(embed);
            return this;
        }

        /// <summary>
        /// Add attachment
        /// </summary>
        /// <param name="filePath">Absolute file path</param>
        /// <param name="fileName">Optional file name parameter to change the file name displayed in discord</param>
        /// <returns></returns>
        public DiscordWebhook AddAttachment(string filePath, string? fileName = null)
        {
            if (!File.Exists(filePath))
                return this;

            if (fileName == null)
                fileName = Path.GetFileName(filePath);

            _attachments.Add(new Attachment(fileName, filePath));
            return this;
        }

        /// <summary>
        /// Add attachment
        /// </summary>
        /// <param name="filePath">Filename</param>
        /// <param name="fileData">File data as byte array</param>
        /// <returns></returns>
        public DiscordWebhook AddAttachment(string filePath, byte[] fileData)
        {
            _attachments.Add(new Attachment(filePath, null, fileData));
            return this;
        }

        /// <summary>
        /// Remove attachment by file name
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public DiscordWebhook RemoveAttachment(string filePath)
        {
            _attachments.RemoveAll(x => x.FilePath == filePath);
            return this;
        }

        /// <summary>
        /// Add a tag to the thread by tag id
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public DiscordWebhook AddTag(ulong tagId)
        {
            _message.Tags.Add(tagId);
            return this;
        }

        /// <summary>
        /// Do not include any embeds when serializing this message
        /// </summary>
        public DiscordWebhook SuppressEmbeds(bool active = true)
        {
            if (_message.Flags == null)
                _message.Flags = MessageFlags.None;

            if (active)
            {
                _message.Flags |= MessageFlags.SuppressEmbeds;
                return this;
            }

            _message.Flags &= ~MessageFlags.SuppressEmbeds;
            return this;
        }

        /// <summary>
        /// This message will not trigger push and desktop notifications
        /// </summary>
        public DiscordWebhook SuppressNotifications(bool active = true)
        {
            if (_message.Flags == null)
                _message.Flags = MessageFlags.None;

            if (active)
            {
                _message.Flags |= MessageFlags.SuppressNotifications;
                return this;
            }

            _message.Flags &= ~MessageFlags.SuppressNotifications;
            return this;
        }

        /// <summary>
        /// The default send timeout is 60 secounds.
        /// </summary>
        /// <param name="timeoutInSecounds">Timeout in secounds</param>
        public DiscordWebhook SetSendTimeout(int timeoutInSecounds)
        {
            _sendTimeout = timeoutInSecounds;

            return this;
        }

        /// <summary>
        /// Send Webhook message (fire and forget)
        /// Disable info about warning: We suppress the warning because we want an asynchronous call without callback or waiting time 
        /// </summary>
        public void Send()
        {
#pragma warning disable 4014
            SendHttpRequestAsync();
#pragma warning restore 4014
        }

        /// <summary>
        /// Send async Webhook message and waits for server confirmation before returns the created message body
        /// </summary>
        public async Task<MessageResponse?> SendAsync()
        {
            return await SendHttpRequestAsync(true);
        }

        /// <summary>
        /// Build webhook url with all required parameter
        /// </summary>
        private string BuildWebhookUrl(bool getMessageBody = false)
        {
            string webhookUrl = _webhookUrl;
            NameValueCollection queryParameter = new NameValueCollection();

            if (_threadId != null && _threadId > 0)
                queryParameter.Add("thread_id", _threadId.Value.ToString());

            if (getMessageBody)
                queryParameter.Add("wait", "1");

            if (queryParameter.Count > 0)
                webhookUrl += queryParameter.ToQueryString();

            return webhookUrl;
        }

        /// <summary>
        /// Send async request to discord api
        /// </summary>
        /// <param name="getMessageBody">Waits for server confirmation of message send before response, and returns the created message body</param>
        private async Task<MessageResponse?> SendHttpRequestAsync(bool getMessageBody = false)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(BuildWebhookUrl(getMessageBody), GetFormData()))
            {
                request.timeout = _sendTimeout; // send timeout in seconds
                request.downloadHandler = new DownloadHandlerBuffer();

                await request.SendWebRequest();
                string? contentBody = null;

                switch (request.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                    case UnityWebRequest.Result.ProtocolError:
                        string error = request.error;
                        if (!string.IsNullOrEmpty(request.downloadHandler?.text))
                            error += "\nResponse: " + request.downloadHandler?.text ?? "";

                        Debug.LogError(error);
                        throw new Exception(error);
                    case UnityWebRequest.Result.Success:
                        contentBody = request.downloadHandler?.text;
                        break;
                }

                if (!getMessageBody || string.IsNullOrEmpty(contentBody))
                    return null;


                var jsonConvertSettings = new JsonSerializerSettings();
                jsonConvertSettings.Converters.Add(new IntegerToColorConverter());

                return JsonConvert.DeserializeObject<MessageResponse>(contentBody ?? "{}", jsonConvertSettings);
            }
        }

        private List<IMultipartFormSection> GetFormData()
        {
            var jsonConvertSettings = new JsonSerializerSettings();
            jsonConvertSettings.Converters.Add(new IntegerToColorConverter());

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("payload_json", JsonConvert.SerializeObject(_message, jsonConvertSettings)));

            int fileCount = 0;
            long fileTotalSize = 0;
            foreach (Attachment attachment in _attachments)
            {
                if (attachment.FileData != null)
                {
                    if (attachment.FileData.Length > ATTACHMENT_DISCORD_MAX_SIZE_IN_BYTES)
                    {
                        Debug.LogError($"Attachmant {attachment.FilePath} size is larger then 25mb");
                        continue;
                    }

                    long newfileTotalSize = fileTotalSize + (long)attachment.FileData.Length;
                    if (newfileTotalSize > ATTACHMENT_DISCORD_MAX_SIZE_IN_BYTES)
                    {
                        Debug.LogError($"The total attachmants size is larger then 25mb. Skip {attachment.FilePath}");
                        continue;
                    }

                    formData.Add(new MultipartFormFileSection($"file{fileCount++}", attachment.FileData, attachment.FileName, null));
                    fileTotalSize = newfileTotalSize;
                    continue;
                }

                if (attachment.FilePath != null && File.Exists(attachment.FilePath))
                {
                    FileInfo fileInfo = new FileInfo(attachment.FilePath);

                    if (fileInfo.Length > ATTACHMENT_DISCORD_MAX_SIZE_IN_BYTES)
                    {
                        Debug.LogError($"Attachmant {attachment.FilePath} size is larger then 25mb");
                        continue;
                    }

                    long newfileTotalSize = fileTotalSize + fileInfo.Length;
                    if (newfileTotalSize > ATTACHMENT_DISCORD_MAX_SIZE_IN_BYTES)
                    {
                        Debug.LogError($"The total attachmants size is larger then 25mb. Skip {attachment.FilePath}");
                        continue;
                    }

                    try
                    {
                        formData.Add(new MultipartFormFileSection($"file{fileCount++}", File.ReadAllBytes(attachment.FilePath), attachment.FileName, null));
                        fileTotalSize = newfileTotalSize;
                    }
                    catch (Exception ex)
                    {
                        // Skip Attachment
                        Debug.LogError($"Can't add attachmant {attachment.FileName} from {attachment.FilePath}:\n{ex}");
                    }
                }
            }

            Reset();

            return formData;
        }

        private void Reset()
        {
            _attachments.Clear();
            _message = new Message();
        }
    }
#nullable disable
}
