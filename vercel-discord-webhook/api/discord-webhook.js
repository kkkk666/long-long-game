const axios = require('axios');

module.exports = async (req, res) => {
  // Enable CORS
  res.setHeader('Access-Control-Allow-Credentials', true);
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET,OPTIONS,POST');
  res.setHeader(
    'Access-Control-Allow-Headers',
    'X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Content-Length, Content-MD5, Content-Type, Date, X-Api-Version'
  );

  // Handle preflight request
  if (req.method === 'OPTIONS') {
    res.status(200).end();
    return;
  }

  // Only allow POST requests
  if (req.method !== 'POST') {
    return res.status(405).json({ error: 'Method not allowed' });
  }

  try {
    const { userName, score, isNewHighScore, currentTopScore } = req.body;

    // Validate required fields
    if (!userName || !score) {
      return res.status(400).json({ error: 'Missing required fields' });
    }

    // Get Discord webhook URL from environment variable
    const webhookUrl = process.env.DISCORD_WEBHOOK_URL;
    if (!webhookUrl) {
      return res.status(500).json({ error: 'Discord webhook URL not configured' });
    }

    // Create message based on score type
    let message = `${userName} has achieved a new high score of ${score}!!!`;

    // Send to Discord
    await axios.post(webhookUrl, {
      username: 'BabyLoongGame',
      content: message
    });

    return res.status(200).json({ success: true, message: 'Message sent to Discord' });
  } catch (error) {
    console.error('Error:', error);
    return res.status(500).json({ error: 'Failed to send message to Discord' });
  }
}; 