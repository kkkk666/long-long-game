using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Leaderboards;
using UnityEngine;

namespace CozyFramework
{
    public class LeaderboardView : MonoBehaviour
    {
        [Header("UI References")]
        public Sprite Top1Icon;
        public Sprite Top2Icon, Top3Icon;
        public Color Top1Color, Top2Color, Top3Color;
        public Color DefaultBackgroundColor = Color.white;

        public Transform LeaderboardEntriesContainer;
        public LeaderboardEntryDisplay LeaderboardEntryDisplay;
        private List<LeaderboardEntryDisplay> currentLeaderboardEntryDisplays = new List<LeaderboardEntryDisplay>();

        public GameObject LeaderboardList;
        public GameObject LeaderboardListContainer;
        public LeaderboardOptionDisplay LeaderboardOptionDisplayPrefab;
        private List<LeaderboardOptionDisplay> leaderboardOptionDisplays = new List<LeaderboardOptionDisplay>();

        public TextMeshProUGUI CurrentlySelectedLeaderboardText;

        private string currentLeaderboard = "";

        public static LeaderboardView Instance;

        private void Awake()
        {
            SingletonHelper.InitializeSingleton(ref Instance, this);
        }

        public void SelectLeaderboardID(string id)
        {
            if (LeaderboardList != null)
                LeaderboardList.SetActive(false);
            if (currentLeaderboard == id) return;
            currentLeaderboard = id;
            UpdateCurrentLeaderBoardText();
            _ = OpenLeaderboard();
        }


        public async Task OpenLeaderboard()
        {
            try
            {
                ClearSlots();

                if (currentLeaderboard == "")
                {
                    if (CozyLeaderboards.Instance == null || CozyLeaderboards.Instance.LeaderboardListTemplate == null
                        || CozyLeaderboards.Instance.LeaderboardListTemplate.Leaderboards == null
                        || CozyLeaderboards.Instance.LeaderboardListTemplate.Leaderboards.Count == 0)
                    {
                        Debug.LogWarning("No leaderboards found in the remote config.");
                        if (MenuManager.Instance != null)
                            MenuManager.Instance.SelectTabByName("Play");
                        return;
                    }

                    currentLeaderboard = CozyLeaderboards.Instance.LeaderboardListTemplate.Leaderboards[0].LeaderboardId;
                }

                UpdateCurrentLeaderBoardText();

                if (LeaderboardsService.Instance == null)
                {
                    Debug.LogWarning("LeaderboardsService.Instance is null.");
                    return;
                }
                var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(currentLeaderboard);
                if (scoresResponse == null || scoresResponse.Results == null)
                {
                    Debug.LogWarning("scoresResponse or Results is null.");
                    return;
                }

                foreach (var player in scoresResponse.Results)
                {
                    if (LeaderboardEntryDisplay == null || LeaderboardEntriesContainer == null) break;
                    var leaderboardEntry = Instantiate(LeaderboardEntryDisplay, LeaderboardEntriesContainer);
                    if (leaderboardEntry != null)
                        leaderboardEntry.Initialize(player.Rank, player.PlayerName, (int)player.Score);
                    currentLeaderboardEntryDisplays.Add(leaderboardEntry);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load the leaderboard: {e.Message}");
            }
        }

        private void UpdateCurrentLeaderBoardText()
        {
            if (CurrentlySelectedLeaderboardText == null || CozyLeaderboards.Instance?.LeaderboardListTemplate?.Leaderboards == null) return;
            var entry = CozyLeaderboards.Instance.LeaderboardListTemplate.Leaderboards.Find(x => x.LeaderboardId == currentLeaderboard);
            if (entry != null)
                CurrentlySelectedLeaderboardText.text = entry.LeaderboardName;
        }

        public void ShowLeaderboardList()
        {
            if (LeaderboardList == null) return;
            LeaderboardList.SetActive(true);

            foreach (var optionDisplay in leaderboardOptionDisplays)
            {
                if (optionDisplay != null && optionDisplay.gameObject != null)
                    Destroy(optionDisplay.gameObject);
            }

            leaderboardOptionDisplays.Clear();

            if (CozyLeaderboards.Instance == null || CozyLeaderboards.Instance.LeaderboardListTemplate == null
                || CozyLeaderboards.Instance.LeaderboardListTemplate.Leaderboards == null) return;
            if (LeaderboardOptionDisplayPrefab == null || LeaderboardListContainer == null) return;
            foreach (var leaderboard in CozyLeaderboards.Instance.LeaderboardListTemplate.Leaderboards)
            {
                var leaderboardOption = Instantiate(LeaderboardOptionDisplayPrefab, LeaderboardListContainer.transform);
                if (leaderboardOption == null) continue;
                if (leaderboardOption.LeaderboardName != null) leaderboardOption.LeaderboardName.text = leaderboard.LeaderboardName;
                if (leaderboardOption.SelectedImage != null) leaderboardOption.SelectedImage.SetActive(currentLeaderboard == leaderboard.LeaderboardId);
                leaderboardOption.Initiliaze(leaderboard.LeaderboardId);

                leaderboardOptionDisplays.Add(leaderboardOption);
            }

        }


        public void ClearSlots()
        {
            foreach (var item in currentLeaderboardEntryDisplays)
            {
                if (item != null && item.gameObject != null)
                    Destroy(item.gameObject);
            }

            currentLeaderboardEntryDisplays.Clear();
        }


    }
}
