using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
#if !UNITY_WEBGL || UNITY_EDITOR
using System.Net;
#endif

namespace CozyFramework
{
    public class CozyPickUsername : MonoBehaviour
    {
        public List<RectTransform> ContentRects = new List<RectTransform>();

        public GameObject PickUsernameView;
        public TMP_InputField UsernameInputField;
        public TMP_Text FeedbackText;
        public Button SubmitButton;
        public Button LoginWithDiscordButton;

        private Button _discordButtonUsed;
        private Button _runtimeDiscordButton;

        private const string DISCORD_AUTH_BASE = "https://loong-loong-game.vercel.app";

#if !UNITY_WEBGL || UNITY_EDITOR
        private HttpListener _httpListener;
        private volatile string _pendingDiscordJson;
#endif

        public static CozyPickUsername Instance;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void OpenDiscordAuthPopup(string url);
#endif

        private void Awake()
        {
            SingletonHelper.InitializeSingleton(ref Instance, this);
            EnsureDiscordLoginReceiverExists();
            ResolvePickUsernameView();
            if (PickUsernameView != null)
                EnsureDiscordButtonExists();
            _discordButtonUsed = _runtimeDiscordButton ?? LoginWithDiscordButton;
            if (_discordButtonUsed != null)
                _discordButtonUsed.onClick.AddListener(OnLoginWithDiscordClick);
        }

        private void ResolvePickUsernameView()
        {
            if (PickUsernameView != null) return;
            var t = transform;
            while (t != null)
            {
                var found = FindChildRecursive(t, "PickUsername");
                if (found != null)
                {
                    PickUsernameView = found.gameObject;
                    return;
                }
                t = t.parent;
            }
            var any = GameObject.Find("PickUsername");
            if (any != null) PickUsernameView = any;
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var c = parent.GetChild(i);
                var found = FindChildRecursive(c, name);
                if (found != null) return found;
            }
            return null;
        }

        private void EnsureDiscordButtonExists()
        {
            if (_runtimeDiscordButton != null) return;
            if (UsernameInputField == null || SubmitButton == null) return;

            Transform container = UsernameInputField.transform.parent;
            RectTransform submitRect = SubmitButton.GetComponent<RectTransform>();
            if (container == null || submitRect == null) return;

            var btnGo = new GameObject("LoginWithDiscordButton");
            btnGo.layer = UsernameInputField.gameObject.layer;

            var rect = btnGo.AddComponent<RectTransform>();
            rect.SetParent(container, false);
            rect.anchorMin = submitRect.anchorMin;
            rect.anchorMax = submitRect.anchorMax;
            rect.pivot = submitRect.pivot;
            rect.sizeDelta = submitRect.sizeDelta;
            rect.anchoredPosition = submitRect.anchoredPosition;

            var img = btnGo.AddComponent<Image>();
            img.raycastTarget = true;
            var submitImg = SubmitButton.GetComponent<Image>();
            if (submitImg != null && submitImg.sprite != null)
            {
                img.sprite = submitImg.sprite;
                img.type = submitImg.type;
            }
            img.color = new Color(0.34f, 0.39f, 0.85f);

            var button = btnGo.AddComponent<Button>();
            button.interactable = true;
            button.targetGraphic = img;

            var textGo = new GameObject("Text");
            textGo.layer = btnGo.layer;
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.SetParent(rect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmpText = textGo.AddComponent<TextMeshProUGUI>();
            tmpText.text = "LOGIN WITH DISCORD";
            tmpText.fontSize = 36;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
            tmpText.raycastTarget = false;
            if (FeedbackText != null)
                tmpText.font = FeedbackText.font;

            _runtimeDiscordButton = button;
        }

        private static void EnsureDiscordLoginReceiverExists()
        {
            if (DiscordLoginReceiver.Instance != null) return;
            var existing = GameObject.Find("DiscordLoginReceiver");
            if (existing != null)
            {
                var c = existing.GetComponent<DiscordLoginReceiver>();
                if (c == null) c = existing.AddComponent<DiscordLoginReceiver>();
                return;
            }
            var go = new GameObject("DiscordLoginReceiver");
            go.AddComponent<DiscordLoginReceiver>();
            DontDestroyOnLoad(go);
        }

        public void ShowPickUsernameView()
        {
            ClearFeedback();
            ResolvePickUsernameView();
            if (PickUsernameView == null) return;

            PickUsernameView.SetActive(true);
            EnsureDiscordButtonExists();
            _discordButtonUsed = _runtimeDiscordButton ?? LoginWithDiscordButton;

            if (UsernameInputField != null)
                UsernameInputField.gameObject.SetActive(false);
            if (SubmitButton != null)
                SubmitButton.gameObject.SetActive(false);

            UpdateTitleText("Please login to continue");

            if (_discordButtonUsed != null)
            {
                _discordButtonUsed.gameObject.SetActive(true);
                _discordButtonUsed.interactable = true;
            }

            StyleCancelButton();

            UpdateRects();
        }

        private void UpdateTitleText(string text)
        {
            if (PickUsernameView == null) return;
            var title = FindChildRecursive(PickUsernameView.transform, "Title");
            if (title != null)
            {
                var tmp = title.GetComponent<TMP_Text>();
                if (tmp != null) tmp.text = text;
            }
        }

        private void StyleCancelButton()
        {
            if (PickUsernameView == null) return;
            var cancelTransform = FindChildRecursive(PickUsernameView.transform, "CancelButton");
            if (cancelTransform == null) return;

            var cancelImg = cancelTransform.GetComponent<Image>();
            if (cancelImg != null)
                cancelImg.color = new Color(0.28f, 0.30f, 0.55f);

            var cancelText = cancelTransform.GetComponentInChildren<TMP_Text>();
            if (cancelText != null)
            {
                cancelText.enableAutoSizing = false;
                cancelText.fontSize = 36;
                cancelText.color = new Color(0.75f, 0.75f, 0.85f);
                cancelText.fontStyle = FontStyles.Normal;
            }

            if (_discordButtonUsed != null)
            {
                var discordRect = _discordButtonUsed.GetComponent<RectTransform>();
                var cancelRect = cancelTransform.GetComponent<RectTransform>();
                if (discordRect != null && cancelRect != null)
                {
                    cancelRect.SetParent(discordRect.parent, false);
                    cancelRect.anchorMin = discordRect.anchorMin;
                    cancelRect.anchorMax = discordRect.anchorMax;
                    cancelRect.pivot = discordRect.pivot;
                    cancelRect.sizeDelta = discordRect.sizeDelta;
                    float btnHeight = discordRect.sizeDelta.y > 10f ? discordRect.sizeDelta.y : discordRect.rect.height;
                    if (btnHeight < 10f) btnHeight = 120f;
                    cancelRect.anchoredPosition = new Vector2(
                        discordRect.anchoredPosition.x,
                        discordRect.anchoredPosition.y - btnHeight - 30f
                    );
                }
            }
        }

        public void OnLoginWithDiscordClick()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string url = DISCORD_AUTH_BASE + "/api/discord-auth";
            OpenDiscordAuthPopup(url);
#else
            StartLocalAuthListener();
#endif
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private void StartLocalAuthListener()
        {
            StopLocalAuthListener();

            var tcp = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            tcp.Start();
            int port = ((System.Net.IPEndPoint)tcp.LocalEndpoint).Port;
            tcp.Stop();

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://localhost:{port}/");
            _httpListener.Start();
            _httpListener.BeginGetContext(OnLocalAuthCallback, null);

            string url = $"{DISCORD_AUTH_BASE}/api/discord-auth?local_port={port}";
            Application.OpenURL(url);
            Debug.Log($"[Discord] Local auth listener on port {port}");
        }

        private void OnLocalAuthCallback(System.IAsyncResult result)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = _httpListener.EndGetContext(result);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Discord] Listener error: {e.Message}");
                return;
            }

            var query = ctx.Request.QueryString;
            string discordId = query["discord_id"];
            string username = query["username"];
            string token = query["token"];
            string error = query["error"];

            string html;
            if (!string.IsNullOrEmpty(error))
                html = "<html><body style='font-family:sans-serif;text-align:center;padding:60px'><h1>Login Failed</h1><p>" + error + "</p></body></html>";
            else
                html = "<html><body style='font-family:sans-serif;text-align:center;padding:60px'><h1>Login Successful!</h1><p>You can close this tab and return to the game.</p></body></html>";

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(html);
            ctx.Response.ContentType = "text/html; charset=utf-8";
            ctx.Response.ContentLength64 = buffer.Length;
            ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
            ctx.Response.Close();

            if (!string.IsNullOrEmpty(discordId) && !string.IsNullOrEmpty(username))
            {
                string safeId = discordId.Replace("\\", "\\\\").Replace("\"", "\\\"");
                string decoded = System.Uri.UnescapeDataString(username);
                string safeName = decoded.Replace("\\", "\\\\").Replace("\"", "\\\"");
                string tokenPart = "";
                if (!string.IsNullOrEmpty(token))
                {
                    string decodedToken = System.Uri.UnescapeDataString(token);
                    string safeToken = decodedToken.Replace("\\", "\\\\").Replace("\"", "\\\"");
                    tokenPart = ",\"token\":\"" + safeToken + "\"";
                }
                _pendingDiscordJson = "{\"type\":\"discord\",\"discord_id\":\"" + safeId + "\",\"username\":\"" + safeName + "\"" + tokenPart + "}";
            }
            else if (!string.IsNullOrEmpty(error))
            {
                string safeErr = error.Replace("\\", "\\\\").Replace("\"", "\\\"");
                _pendingDiscordJson = "{\"type\":\"discord\",\"error\":\"" + safeErr + "\"}";
            }

            StopLocalAuthListener();
        }

        private void StopLocalAuthListener()
        {
            try
            {
                if (_httpListener != null && _httpListener.IsListening)
                {
                    _httpListener.Stop();
                    _httpListener.Close();
                }
            }
            catch { }
            _httpListener = null;
        }

        private void Update()
        {
            if (_pendingDiscordJson != null)
            {
                string json = _pendingDiscordJson;
                _pendingDiscordJson = null;
                if (DiscordLoginReceiver.Instance != null)
                    DiscordLoginReceiver.Instance.OnDiscordUser(json);
            }
        }

        private void OnDestroy()
        {
            StopLocalAuthListener();
        }
#endif

        public void OnDiscordLoginError(string error)
        {
            if (FeedbackText != null)
            {
                FeedbackText.text = "Discord login failed. Please try again.";
                FeedbackText.gameObject.SetActive(true);
            }
            UpdateRects();
        }

        public void OnDiscordLoginSuccess()
        {
            if (LeaderboardView.Instance != null)
                _ = LeaderboardView.Instance.OpenLeaderboard();
            if (CozyLeaderboards.Instance != null && CozyLeaderboards.Instance.LeaderboardExists("highscore") && PlayerManager.Instance != null)
                _ = CozyAPI.Instance.SubmitScoreToLeaderboard("highscore", PlayerManager.Instance.PlayerHighScore);
            if (CozyLeaderboards.Instance != null && CozyLeaderboards.Instance.LeaderboardExists("matches") && PlayerManager.Instance != null)
                _ = CozyAPI.Instance.SubmitScoreToLeaderboard("matches", PlayerManager.Instance.PlayerTotalMatches);
        }

        private void UpdateRects()
        {
            foreach (var rect in ContentRects)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }

        public void HidePickUsernameView()
        {
            if (PickUsernameView != null)
                PickUsernameView.SetActive(false);
        }

        private void ClearFeedback()
        {
            if (FeedbackText != null)
            {
                FeedbackText.text = string.Empty;
                FeedbackText.gameObject.SetActive(false);
            }
        }
    }
}
