using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyFramework
{
    public class MenuManager : MonoBehaviour
    {
        [Header("UI Views")]
        public GameObject AuthenticationView;
        public GameObject GameUIView;
        public GameObject MainMenuView;
        public GameObject InGameView;
        public GameObject BottomBar;

        [Header("UI Components")]
        public CanvasGroup CurrenciesCG;

        [Header("Menu Tab Colors")]
        public Color MenuTabBackgroundOnColor;
        public Color MenuTabBackgroundOffColor;

        public Color MenuTabTextOnColor;
        public Color MenuTabTextOffColor;

        public Color MenuTabIconOnColor;
        public Color MenuTabIconOffColor;

        [System.Serializable]
        public class MenuTab
        {
            public string TabName;
            public Image BackgroundImage;
            public TextMeshProUGUI TabNameText;
            public Image TabIcon;

            public List<GameObject> TabGameObjects;
            public List<CanvasGroup> TabCanvasGroups;
        }

        [Header("Menu Tabs Configuration")]
        public List<MenuTab> MenuTabs;

        private int currentTabIndex = -1;

        public static MenuManager Instance;

        private void Awake()
        { 
            SingletonHelper.InitializeSingleton(ref Instance, this);
            
            AuthenticationView.SetActive(true);

            foreach (var tab in MenuTabs)
            {
                foreach (var go in tab.TabGameObjects)
                {
                    go.SetActive(false);
                }

                foreach (var cg in tab.TabCanvasGroups)
                {
                    CozyUtilities.DisableCG(cg);
                }
            }
        }

        private void Start()
        {
            CozyEvents.PlayerInitialized += InitializeMainMenu;
        }

        private void OnDisable()
        {
            CozyEvents.PlayerInitialized -= InitializeMainMenu;
        }

        private void InitializeMainMenu()
        {
            AuthenticationView.SetActive(false);
            GameUIView.SetActive(true);
            MainMenuView.SetActive(true);
            InGameView.SetActive(false);
            
            // Show username prompt if player doesn't have a username
            if (!PlayerManager.Instance.PlayerHasUsername)
            {
                CozyPickUsername.Instance.ShowPickUsernameView();
            }
            
            SelectTabByName("Play");
            BottomBar.SetActive(true);
            UpdateCurrenciesVisibility(true);
        }

        public void StartGame()
        {
            if (!PlayerManager.Instance.PlayerHasUsername)
            {
                CozyPickUsername.Instance.ShowPickUsernameView();
                return;
            }
            GameUIView.SetActive(true);
            MainMenuView.SetActive(false);
            InGameView.SetActive(true);
        }

        public void EndGame()
        {
            // Reset UI states
            GameUIView.SetActive(true);
            MainMenuView.SetActive(true);
            InGameView.SetActive(false);
            
            // Reset any game-specific UI elements
            if (GameUIView != null)
            {
                foreach (Transform child in GameUIView.transform)
                {
                    if (child.name.Contains("ENDSCREEN"))
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
            
            // Reset currencies visibility
            UpdateCurrenciesVisibility(true);
        }

        private void UpdateCurrenciesVisibility(bool visible)
        {
            if (visible)
            {
                CozyUtilities.EnableCG(CurrenciesCG);
            }
            else
            {
                CozyUtilities.DisableCG(CurrenciesCG);
            }
        }

        private async Task TabOpened(string tabName)
        {
            switch (tabName)
            {
                case "Ranks":
                    UpdateCurrenciesVisibility(false);

                    if (!PlayerManager.Instance.PlayerHasUsername)
                    {
                        CozyPickUsername.Instance.ShowPickUsernameView();
                    }
                    else
                    {
                        await LeaderboardView.Instance.OpenLeaderboard();
                    }

                    break;
                case "Shop":
                    UpdateCurrenciesVisibility(true);
                    ShopView.Instance.OpenShop();
                    break;
                case "Play":
                    UpdateCurrenciesVisibility(true);
                    break;
            }
        }

        private void TabClosed(string tabName)
        {
            switch (tabName)
            {
                case "Ranks":
                    
                    break;
                case "Shop":
                    
                    break;
                case "Play":
                    
                    break;
            }
        }

        public void SelectTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= MenuTabs.Count || tabIndex == currentTabIndex)
                return;

            if (currentTabIndex != -1) 
                TabClosed(MenuTabs[currentTabIndex].TabName);

            for (int i = 0; i < MenuTabs.Count; i++)
            {
                bool isSelected = (i == tabIndex);
                MenuTab tab = MenuTabs[i];

                if (tab.BackgroundImage != null)
                {
                    tab.BackgroundImage.color = isSelected ? MenuTabBackgroundOnColor : MenuTabBackgroundOffColor;
                }

                if (tab.TabNameText != null)
                {
                    tab.TabNameText.color = isSelected ? MenuTabTextOnColor : MenuTabTextOffColor;
                }

                if (tab.TabIcon != null)
                {
                    tab.TabIcon.color = isSelected ? MenuTabIconOnColor : MenuTabIconOffColor;
                }

                foreach (var go in tab.TabGameObjects)
                {
                    go.SetActive(isSelected);
                }

                foreach (var cg in tab.TabCanvasGroups)
                {
                    if (isSelected)
                        CozyUtilities.EnableCG(cg);
                    else
                        CozyUtilities.DisableCG(cg);
                }
            }

            _ = TabOpened(MenuTabs[tabIndex].TabName);
            currentTabIndex = tabIndex;
        }

        public void SelectTabByName(string tabName)
        {
            for (int i = 0; i < MenuTabs.Count; i++)
            {
                if (MenuTabs[i].TabName == tabName)
                {
                    SelectTab(i);
                    return;
                }
            }
        }
    }
}
