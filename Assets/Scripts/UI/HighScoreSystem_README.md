# High Score System Setup Guide

This guide will help you set up a high score system for your game, allowing players to save their scores and view a leaderboard.

## Overview

The high score system consists of the following components:

1. **HighScoreManager**: Handles saving and loading high scores using PlayerPrefs.
2. **HighScoreUI**: Displays the high scores and handles player name input.
3. **EndScreenSetup**: Configures the end screen with high score UI elements.
4. **HighScoreEntry Prefab**: A UI prefab for displaying individual high score entries.

## Setup Instructions

### Step 1: Add the HighScoreManager

1. Create a new GameObject in your scene named "HighScoreManager".
2. Add the `HighScoreManager.cs` script to this GameObject.
3. Make sure this GameObject is marked as "DontDestroyOnLoad" (the script handles this automatically).

### Step 2: Set Up the ENDSCREEN Canvas

1. Locate your existing ENDSCREEN Canvas GameObject.
2. Add the following UI elements as children:
   - A Panel named "HighScorePanel"
   - A Vertical Layout Group named "HighScoreContainer" inside the panel
   - A TMP_InputField for the player name input
   - A Button for submitting the high score
   - A TextMeshProUGUI element for displaying the final score
   - Buttons for restarting the game and returning to the main menu

### Step 3: Create the HighScoreEntry Prefab

1. Create a new UI GameObject with a Horizontal Layout Group component.
2. Name it "HighScoreEntry".
3. Configure the Horizontal Layout Group:
   - Padding: Left=10, Right=10, Top=5, Bottom=5
   - Child Alignment: Middle Left
   - Spacing: 10
   - Child Force Expand: Width=false, Height=false
   - Control Child Size: Width=true, Height=true

4. Add four TextMeshProUGUI components as children:
   - Child 1: "RankText" - Width=50, Height=30
   - Child 2: "NameText" - Width=200, Height=30
   - Child 3: "ScoreText" - Width=100, Height=30
   - Child 4: "DateText" - Width=100, Height=30

5. Configure each TextMeshProUGUI component with appropriate styling.
6. Add a background Image component to the root GameObject.
7. Save this as a prefab in your project.

### Step 4: Add Scripts to the ENDSCREEN Canvas

1. Add the `EndScreenSetup.cs` script to your ENDSCREEN Canvas GameObject.
2. In the Inspector, assign all the UI elements you created to the corresponding fields:
   - High Score Panel
   - High Score Container
   - Player Name Input
   - Submit Button
   - Final Score Text
   - High Score Entry Prefab
   - Restart Button
   - Main Menu Button

3. The `EndScreenSetup.cs` script will automatically add the `HighScoreUI.cs` component if it doesn't exist.

### Step 5: Update PlayerDeath.cs

The `PlayerDeath.cs` script has been modified to initialize the high score UI when the game ends. Make sure you're using the updated version of this script.

## How It Works

1. When the player runs out of lives, the ENDSCREEN Canvas is activated.
2. The `HighScoreUI` component checks if the player's score qualifies as a high score.
3. If it's a high score, the player is prompted to enter their name.
4. After submitting their name, the high score is saved and the leaderboard is displayed.
5. If it's not a high score, the leaderboard is displayed immediately.
6. The player can restart the game or return to the main menu using the provided buttons.

## Customization

You can customize the appearance of the high score UI by modifying the prefabs and UI elements. You can also adjust the maximum number of high scores displayed by changing the `maxDisplayedScores` field in the `HighScoreUI.cs` script.

## Troubleshooting

- If high scores are not being saved, check that the `HighScoreManager` GameObject exists in your scene.
- If the high score UI is not appearing, make sure all the UI elements are correctly assigned in the `EndScreenSetup` component.
- If the high score entry prefab is not displaying correctly, check that it has all the required TextMeshProUGUI components.

## Additional Notes

- High scores are saved using PlayerPrefs, which means they will persist between game sessions.
- The high score system is designed to work with the existing game over system in your game.
- You can access the current high scores programmatically using `HighScoreManager.Instance.GetHighScores()`. 