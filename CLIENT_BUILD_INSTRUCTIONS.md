# Baby Dragon - Client Build Instructions

This document explains the steps required to complete the WebGL build with your Unity Services configuration.

---

## What Has Been Completed

The following code changes have been made and pushed to the repository:

| Configuration | Value |
|---------------|-------|
| **Vercel Webhook Endpoint** | `https://loong-loong-game.vercel.app/api/discord-webhook` |
| **API Key** | `G5p9kVn7zR2tC0yB1mQ8sXeLfUwD3hJ4` |
| **Leaderboard ID** | `Babylon-Loong` |

The webhook is configured to **only trigger for new high scores**.

---

## What You Need To Do

### Step 1: Clone or Pull the Latest Code

If you haven't already:
```bash
git clone https://github.com/k3nny-Wood/BabyDragon2D.git
```

Or if you have the repo:
```bash
git pull origin main
```

### Step 2: Open Project in Unity

- Open Unity Hub
- Add the project folder
- Open with Unity (recommended version: 2022.3 LTS or later)

### Step 3: Link to Your Unity Services Project

This is the critical step that requires your account access:

1. In Unity, go to **Edit** → **Project Settings** → **Services**
2. Click **Link to Unity Project** (or unlink first if already linked)
3. Select your organization
4. Select or enter your project:
   - **Project ID:** `7438f9c7-8608-401f-8300-3d6d8783c97b`
   - **Leaderboard ID:** `Babylon-Loong`

### Step 4: Verify Services Are Enabled

In Project Settings → Services, ensure these are enabled:
- **Authentication** - Required for user login
- **Leaderboards** - Required for high score tracking
- **Cloud Save** - Required for player data persistence

### Step 5: Build for WebGL

1. Go to **File** → **Build Settings**
2. Select **WebGL** platform
3. Click **Switch Platform** (if not already on WebGL)
4. Click **Build** or **Build and Run**
5. Choose an output folder

---

## Verification Checklist

After building, verify:

- [ ] Game loads in browser
- [ ] Player can create username
- [ ] Score increments when collecting coins
- [ ] Game over screen appears when lives = 0
- [ ] High scores are submitted to your leaderboard
- [ ] Discord notification appears for new high scores

---

## Testing the Discord Webhook

You can test the webhook directly:

```bash
curl -X POST "https://loong-loong-game.vercel.app/api/discord-webhook" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "userName=TestPlayer&score=9999&apiKey=G5p9kVn7zR2tC0yB1mQ8sXeLfUwD3hJ4"
```

Expected response:
```json
{"success":true,"message":"Message sent to Discord"}
```

---

## File Reference

Key files that were modified:

| File | Changes |
|------|---------|
| `Assets/Scripts/Gameplay/PlayerDeath.cs` | Webhook URL, API key, Leaderboard ID |

---

## Troubleshooting

### "Leaderboard not found" error
- Ensure the leaderboard `Babylon-Loong` exists in your Unity Dashboard
- Verify the project is correctly linked in Unity Services

### Discord notification not appearing
- Check the Vercel function logs at your Vercel dashboard
- Verify the `DISCORD_WEBHOOK_URL` environment variable is set
- Test with the curl command above

### Authentication errors
- Ensure Authentication service is enabled in Unity Dashboard
- Check that anonymous sign-in is enabled

---

## Support

For questions about:
- **Code changes:** Contact the development team
- **Unity Services:** [Unity Dashboard](https://dashboard.unity3d.com)
- **Vercel Webhook:** Check function logs at [vercel.com](https://vercel.com)

---

*Document prepared: February 2025*
