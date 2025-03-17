using Newtonsoft.Json;

#nullable enable
namespace ShadowGroveGames.WebhooksForDiscord.Scripts.DTO
{
    public readonly struct Attachment
    {
        /// <summary>
        /// File name
        /// </summary>
        public readonly string FileName;

        /// <summary>
        /// Absolute file path
        /// </summary>
        public readonly string? FilePath;

        /// <summary>
        /// Raw file bytes
        /// </summary>
        public readonly byte[]? FileData;

        public Attachment(string fileName, string? filePath = null, byte[]? fileData = null)
        {
            FileName = fileName;
            FilePath = filePath;
            FileData = fileData;
        }
    }
}
#nullable disable