using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyFramework
{
    public class LeaderboardEntryDisplay : MonoBehaviour
    {
        [Header("UI References")] public TextMeshProUGUI RankText;
        public TextMeshProUGUI PlayerNameText;
        public TextMeshProUGUI ScoreText;
        public Image TopIcon;
        public Image BackgroundImage;

        public void Initialize(int position, string playerName, int score)
        {
            if (ScoreText != null) ScoreText.text = score.ToString();
            if (PlayerNameText != null) PlayerNameText.text = playerName ?? "";

            if (LeaderboardView.Instance == null)
            {
                if (RankText != null) { RankText.gameObject.SetActive(true); RankText.text = (position + 1).ToString(); }
                return;
            }

            if (position >= 0 && position <= 2)
            {
                if (TopIcon != null) TopIcon.gameObject.SetActive(true);
                switch (position)
                {
                    case 0:
                        if (RankText != null) RankText.gameObject.SetActive(false);
                        if (TopIcon != null && LeaderboardView.Instance.Top1Icon != null) TopIcon.sprite = LeaderboardView.Instance.Top1Icon;
                        if (BackgroundImage != null) BackgroundImage.color = LeaderboardView.Instance.Top1Color;
                        break;
                    case 1:
                        if (RankText != null) RankText.gameObject.SetActive(false);
                        if (TopIcon != null && LeaderboardView.Instance.Top2Icon != null) TopIcon.sprite = LeaderboardView.Instance.Top2Icon;
                        if (BackgroundImage != null) BackgroundImage.color = LeaderboardView.Instance.Top2Color;
                        break;
                    case 2:
                        if (RankText != null) RankText.gameObject.SetActive(false);
                        if (TopIcon != null && LeaderboardView.Instance.Top3Icon != null) TopIcon.sprite = LeaderboardView.Instance.Top3Icon;
                        if (BackgroundImage != null) BackgroundImage.color = LeaderboardView.Instance.Top3Color;
                        break;
                }
            }
            else
            {
                if (TopIcon != null) TopIcon.gameObject.SetActive(false);
                if (RankText != null)
                {
                    RankText.gameObject.SetActive(true);
                    RankText.text = (position + 1).ToString();
                }
                if (BackgroundImage != null) BackgroundImage.color = LeaderboardView.Instance.DefaultBackgroundColor;
            }
        }

    }
}
