# Baby Dragon - Discord Webhook Deployment Guide

This guide explains how to deploy the Discord webhook system for the Baby Dragon game, including security features and how the game integrates with the webhook.

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Deployment Steps](#deployment-steps)
4. [Environment Variables](#environment-variables)
5. [Security Features](#security-features)
6. [How It Works](#how-it-works)
7. [Testing](#testing)
8. [Troubleshooting](#troubleshooting)

---

## Overview

The Baby Dragon game uses a Vercel serverless function to post high score notifications to Discord. When a player achieves a new high score, the game sends a request to the Vercel webhook, which then posts a message to your Discord channel.

**Architecture:**
```
Unity Game (WebGL)
    ↓ HTTP POST
Vercel Serverless Function
    ↓ HTTP POST
Discord Webhook
    ↓
Discord Channel Message
```

---

## Prerequisites

Before deploying, you need:

1. **A Vercel Account** - Sign up at [vercel.com](https://vercel.com)
2. **A Discord Server** with permission to create webhooks
3. **Node.js** installed locally (for Vercel CLI)
4. **The webhook source code** (located in `vercel-discord-webhook/` folder)

---

## Deployment Steps

### Step 1: Create a Discord Webhook

1. Open your Discord server
2. Go to **Server Settings** → **Integrations** → **Webhooks**
3. Click **New Webhook**
4. Name it (e.g., "BabyDragon High Scores")
5. Select the channel where messages should appear
6. Click **Copy Webhook URL** - save this for later
7. Click **Save Changes**

### Step 2: Install Vercel CLI

Open a terminal and run:
```bash
npm install -g vercel
```

### Step 3: Login to Vercel

```bash
vercel login
```

Select your login method (GitHub, Email, etc.) and complete authentication.

### Step 4: Deploy the Webhook

Navigate to the webhook folder and deploy:
```bash
cd vercel-discord-webhook
vercel deploy --prod
```

Answer the prompts:
- **Link to existing project?** → No (for first deployment)
- **Project name?** → `babydragon-webhook` (or your preferred name, must be lowercase)
- **Directory?** → Press Enter (use current directory)
- **Modify settings?** → No

You'll receive a production URL like:
```
https://babydragon-webhook.vercel.app
```

### Step 5: Set Environment Variables

#### Option A: Via Vercel CLI
```bash
vercel env add DISCORD_WEBHOOK_URL
```
→ Enter your Discord webhook URL from Step 1
→ Select all environments (Production, Preview, Development)

```bash
vercel env add API_SECRET
```
→ Enter: `BabyDragon2024SecureKey` (or your custom secret)
→ Select all environments

#### Option B: Via Vercel Dashboard
1. Go to [vercel.com](https://vercel.com) → Your Project → **Settings** → **Environment Variables**
2. Add the following:

| Name | Value |
|------|-------|
| `DISCORD_WEBHOOK_URL` | Your Discord webhook URL |
| `API_SECRET` | `BabyDragon2024SecureKey` |

### Step 6: Redeploy with Environment Variables

After adding environment variables, redeploy:
```bash
vercel --prod
```

### Step 7: Update Unity Code (If Using a New URL)

If your Vercel URL is different from the default, update it in:

**File:** `Assets/Scripts/Gameplay/PlayerDeath.cs`

Find and replace the endpoint URL (appears twice, around lines 251 and 305):
```csharp
string vercelEndpoint = "https://YOUR-PROJECT.vercel.app/api/discord-webhook";
```

Also update the API key constant if you changed it:
```csharp
private const string WEBHOOK_API_KEY = "YourNewSecretKey";
```

---

## Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `DISCORD_WEBHOOK_URL` | Yes | The full Discord webhook URL |
| `API_SECRET` | Yes | Secret key that must match the Unity code |

**Important:** The `API_SECRET` in Vercel must exactly match `WEBHOOK_API_KEY` in `PlayerDeath.cs`.

---

## Security Features

The webhook includes multiple security layers to prevent abuse:

### 1. API Key Authentication
- Every request must include a valid `apiKey` field
- Requests without a matching key are rejected with `401 Unauthorized`
- The key is checked against the `API_SECRET` environment variable

### 2. Rate Limiting
- Maximum **5 requests per minute** per IP address
- Prevents spam attacks and Discord rate limit issues
- Returns `429 Too Many Requests` when exceeded

### 3. Input Sanitization
- **Username:** Limited to 20 characters, stripped of Discord mentions (`@everyone`, `@here`), and special characters
- **Score:** Must be a positive integer between 0 and 999,999,999
- Prevents Discord injection attacks

### 4. Request Validation
- Only `POST` requests are accepted
- Required fields: `userName`, `score`, `apiKey`
- Missing fields return `400 Bad Request`

### 5. Conditional Notifications
- Discord only receives messages for **new high scores**
- Prevents channel spam from every game session

### 6. Production Debug Stripping
- All `Debug.Log` statements in Unity are wrapped with `#if UNITY_EDITOR || DEVELOPMENT_BUILD`
- Release builds have zero logging overhead
- Sensitive information not exposed in browser console

---

## How It Works

### Game Flow

1. **Player dies with 0 lives remaining**
   - `PlayerDeath.cs` → `Execute()` method is called

2. **Check for new high score**
   - Query Unity Leaderboards for current top score
   - Compare with player's final score

3. **Submit score to leaderboard**
   - Score is submitted regardless of whether it's a high score

4. **Send Discord notification (if new high score)**
   - Only if `isNewHighScore == true`
   - POST request to Vercel endpoint with:
     - `userName` - Player's username
     - `score` - Final score
     - `apiKey` - Authentication key
     - `isNewHighScore` - Boolean flag
     - `currentTopScore` - Previous top score

5. **Vercel webhook receives request**
   - Validates API key
   - Checks rate limit
   - Sanitizes input
   - Posts to Discord

6. **Discord displays message**
   - Format: `"🐉 {username} has achieved a new high score of {score}!!!"`

### Code Location

| Component | File | Key Lines |
|-----------|------|-----------|
| Death handling | `Assets/Scripts/Gameplay/PlayerDeath.cs` | Line 54-185 |
| Webhook call | `Assets/Scripts/Gameplay/PlayerDeath.cs` | Line 250-279 |
| API key constant | `Assets/Scripts/Gameplay/PlayerDeath.cs` | Line 23 |
| Vercel function | `vercel-discord-webhook/api/discord-webhook.js` | All |

---

## Testing

### Test the Webhook Directly

After deployment, test with curl:

```bash
curl -X POST "https://YOUR-PROJECT.vercel.app/api/discord-webhook" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "userName=TestUser&score=9999&apiKey=BabyDragon2024SecureKey"
```

**Expected responses:**

| Response | Meaning |
|----------|---------|
| `{"success":true,"message":"Message sent to Discord"}` | Success! Check Discord |
| `{"error":"Unauthorized"}` | Wrong API key |
| `{"error":"Server configuration error"}` | Missing environment variables |
| `{"error":"Discord webhook URL not configured"}` | Missing `DISCORD_WEBHOOK_URL` |
| `{"error":"Too many requests..."}` | Rate limited |

### Test from Unity

1. Open the game in Unity Editor
2. Play the game and collect some coins
3. Die intentionally until you run out of lives
4. Check the Unity Console for debug messages
5. Check your Discord channel for the notification

---

## Troubleshooting

### "Server configuration error"
- **Cause:** Environment variables not set
- **Fix:** Add `API_SECRET` and `DISCORD_WEBHOOK_URL` in Vercel, then redeploy

### "Unauthorized"
- **Cause:** API key mismatch
- **Fix:** Ensure `API_SECRET` in Vercel matches `WEBHOOK_API_KEY` in Unity code

### "Discord webhook URL not configured"
- **Cause:** Missing `DISCORD_WEBHOOK_URL` environment variable
- **Fix:** Add the Discord webhook URL in Vercel settings

### No message appears in Discord
1. Check the webhook URL is correct
2. Verify the Discord channel exists
3. Test the webhook directly with curl
4. Check Vercel function logs at: `vercel.com` → Project → **Deployments** → **Functions**

### "Authentication Required" when testing
- **Cause:** Vercel deployment protection is enabled
- **Fix:** Go to Project Settings → Deployment Protection → Set to "Standard Protection"

### Rate limited
- **Cause:** Too many requests in 1 minute
- **Fix:** Wait 60 seconds and try again

---

## File Structure

```
vercel-discord-webhook/
├── api/
│   └── discord-webhook.js    # Main serverless function
├── package.json              # Dependencies (axios)
├── vercel.json               # Vercel configuration
└── DEPLOYMENT_GUIDE.md       # This file
```

---

## Support

For issues with:
- **Vercel deployment:** [vercel.com/docs](https://vercel.com/docs)
- **Discord webhooks:** [Discord Developer Docs](https://discord.com/developers/docs/resources/webhook)
- **Unity integration:** Check `PlayerDeath.cs` comments and debug logs

---

*Last updated: January 2025*
