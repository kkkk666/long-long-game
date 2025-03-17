using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
    
    private const int MAX_HIGH_SCORES = 10;
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "highscores.json");
    
    private HighScoreData highScoreData;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighScores();
            
            // If no high scores exist, create dummy scores
            if (highScoreData.highScores.Count == 0)
            {
                CreateDummyScores();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void CreateDummyScores()
    {
        // Add 10 dummy scores
        AddHighScore("Dragon Master", 10000);
        AddHighScore("Coin Hunter", 8500);
        AddHighScore("Jewel Seeker", 7200);
        AddHighScore("Speed Runner", 6800);
        AddHighScore("Pro Gamer", 5500);
        AddHighScore("Adventure Kid", 4200);
        AddHighScore("Lucky Player", 3800);
        AddHighScore("Treasure Hunter", 3000);
        AddHighScore("Casual Gamer", 2500);
        AddHighScore("Beginner", 1500);
    }
    
    public void LoadHighScores()
    {
        if (File.Exists(SaveFilePath))
        {
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                highScoreData = JsonUtility.FromJson<HighScoreData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading high scores: " + e.Message);
                highScoreData = new HighScoreData();
            }
        }
        else
        {
            highScoreData = new HighScoreData();
        }
    }
    
    public void SaveHighScores()
    {
        try
        {
            string json = JsonUtility.ToJson(highScoreData, true); // true for pretty print
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"High scores saved to: {SaveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving high scores: " + e.Message);
        }
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
        if (highScoreData.highScores.Count < MAX_HIGH_SCORES)
            return true;
            
        return score > highScoreData.highScores.Min(h => h.score);
    }
    
    // Method to clear all high scores (useful for testing)
    public void ClearHighScores()
    {
        highScoreData.highScores.Clear();
        SaveHighScores();
    }
} 