using System;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace CozyFramework
{
    public class CozyAuthentication : MonoBehaviour
    {
        public enum AuthenticationType
        {
            Anonymous,
        }

        public AuthenticationType AuthType;
        
        public static CozyAuthentication Instance;

        private void Awake()
        {
            SingletonHelper.InitializeSingleton(ref Instance, this);
        }

        async void Start()
        {
            try
            {
                await InitializePlayerAccount();
            }
            catch (AuthenticationException authEx)
            {
                await HandleAuthenticationException(authEx);
            }
            catch (Exception e)
            {
                Debug.LogError($"Authentication failed: {e.Message}");
                Debug.LogException(e);
            }
        }

        private async Task InitializePlayerAccount()
        {
            try
            {
                var options = new InitializationOptions();
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                options.SetProfile("WebGL");
                #endif

                await UnityServices.InitializeAsync(options);
                if (this == null) return;

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await SignInAsync();
                    if (this == null) return;
                }

                AnalyticsService.Instance.StartDataCollection();
                CozyEvents.OnAuthSuccess();

                var refreshEconomyTask = CozyEconomy.Instance.RefreshEconomyConfiguration();
                var refreshCurrencyTask = CozyEconomy.Instance.RefreshCurrencyBalances();
                
                await Task.WhenAll(
                    refreshEconomyTask,
                    refreshCurrencyTask
                );
                if (this == null) return;
                
                CozyEvents.OnPlayerInitialized();
                Debug.Log("Authentication: <color=green>SUCCESSFUL</color>");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
                throw;
            }
        }

        private async Task SignInAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Player signed in successfully");
            }
            catch (AuthenticationException e)
            {
                Debug.LogError($"Failed to sign in: {e.Message}");
                throw;
            }
        }

        private async Task HandleAuthenticationException(AuthenticationException authEx)
        {
            Debug.LogError($"Authentication error: {authEx.Message}");
            // You might want to add retry logic here
        }
    }
}
