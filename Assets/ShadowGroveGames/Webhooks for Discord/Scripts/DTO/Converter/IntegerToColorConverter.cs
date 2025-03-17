using Newtonsoft.Json;
using System;
using UnityEngine;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts.DTO
{
    internal class IntegerToColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            if (value == null)
                return;

            writer.WriteValue(Convert.ToInt32(ColorUtility.ToHtmlStringRGB(value), 16));
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return UnityEngine.Color.white;

            int intColor = int.Parse(reader.Value.ToString());
            string hexColor = String.Format("#{0:X6}", 0xFFFFFF & intColor).ToUpper();

            if (!ColorUtility.TryParseHtmlString(hexColor, out Color color))
                throw new Exception("Cant convert to Color");

            return color;
        }
    }
}
