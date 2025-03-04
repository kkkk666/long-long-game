using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class HighScoreEntry
{
    public string playerName;
    public int score;
    public string date;

    public HighScoreEntry(string playerName, int score)
    {
        this.playerName = playerName;
        this.score = score;
        this.date = DateTime.Now.ToString("MM/dd/yyyy");
    }
}

[Serializable]
public class HighScoreData
{
    public List<HighScoreEntry> highScores = new List<HighScoreEntry>();
}

public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager Instance { get; private set; }
    
    private const string HIGH_SCORE_KEY = "HighScores";
    private const int MAX_HIGH_SCORES = 10;
    
    private HighScoreData highScoreData;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void LoadHighScores()
    {
        string json = PlayerPrefs.GetString(HIGH_SCORE_KEY, "");
        
        if (string.IsNullOrEmpty(json))
        {
            highScoreData = new HighScoreData();
        }
        else
        {
            try
            {
                highScoreData = JsonUtility.FromJson<HighScoreData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading high scores: " + e.Message);
                highScoreData = new HighScoreData();
            }
        }
    }
    
    public void SaveHighScores()
    {
        string json = JsonUtility.ToJson(highScoreData);
        PlayerPrefs.SetString(HIGH_SCORE_KEY, json);
        PlayerPrefs.Save();
    }
    
    public bool AddHighScore(string playerName, int score)
    {
        // Check if this score qualifies as a high score
        if (highScoreData.highScores.Count < MAX_HIGH_SCORES || score > highScoreData.highScores.Min(h => h.score))
        {
            // Add the new high score
            highScoreData.highScores.Add(new HighScoreEntry(playerName, score));
            
            // Sort the high scores in descending order
            highScoreData.highScores = highScoreData.highScores
                .OrderByDescending(h => h.score)
                .Take(MAX_HIGH_SCORES)
                .ToList();
            
            // Save the updated high scores
            SaveHighScores();
            
            return true;
        }
        
        return false;
    }
    
    public List<HighScoreEntry> GetHighScores()
    {
        return highScoreData.highScores;
    }
    
    public bool IsHighScore(int score)
    {
        return highScoreData.highScores.Count < MAX_HIGH_SCORES || score > highScoreData.highScores.Min(h => h.score);
    }
} 