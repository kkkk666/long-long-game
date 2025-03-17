using System.Collections.Specialized;
using System.Linq;

namespace ShadowGroveGames.WebhooksForDiscord.Scripts.Extentions
{
    public static class NameValueCollectionExtention
    {
        public static string ToQueryString(this NameValueCollection nvc)
        {
            var array = (
                from key in nvc.AllKeys
                from value in nvc.GetValues(key)
                select $"{key}={value}"
            ).ToArray();

            return "?" + string.Join("&", array);
        }
    }
}
