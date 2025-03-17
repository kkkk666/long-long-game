#if UNITY_EDITOR
namespace ShadowGroveGames.WebhooksForDiscord.Editor.Setup
{
    internal static class Asset
    {
        internal const string KEY = "WebhooksForDiscord";
        internal const string NAME = "Webhooks for Discord";
        internal const string LOGO = "webhooks-for-discord-banner";
        internal const string REVIEW_URL = "https://assetstore.unity.com/packages/tools/integration/webhooks-for-discord-246019?utm_source=editor#reviews";
        internal const string README_GUID = "cd90693397bfee8459a0e355a65c0f27";

        internal readonly static string[] DONT_SHOW_IF_ASSABMLY_LOADED = new string[]
        {
            "org.Shadow-Grove.CompleteToolboxForDiscord.Editor",
            "org.Shadow-Grove.FeedbackOverDiscord.Editor",
        };

        // Review
        internal const int REVIEW_MIN_OPENINGS = 2;
        internal const int REVIEW_MIN_DAYS = 10;

        // Editor Prefs
        internal const string EDITOR_PREFS_KEY_GETTING_STARTED = KEY + "-GettingStarted";
        internal const string EDITOR_PREFS_KEY_REVIEW_DISABLE_REMINDER = KEY + "-ReviewReminder";
        internal const string EDITOR_PREFS_KEY_REVIEW_EDITOR_OPEN_COUNT = KEY + "-ReviewEditorOpenCount";
        internal const string EDITOR_PREFS_KEY_REVIEW_INIT_DATE = KEY + "-ReviewInitDate";
    }
}
#endif