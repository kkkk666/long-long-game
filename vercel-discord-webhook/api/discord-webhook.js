const axios = require('axios');

// Simple in-memory rate limiting (resets on cold start, but still helps)
const rateLimitMap = new Map();
const RATE_LIMIT_WINDOW_MS = 60000; // 1 minute
const MAX_REQUESTS_PER_WINDOW = 5; // Max 5 requests per minute per IP

function isRateLimited(ip) {
  const now = Date.now();
  const record = rateLimitMap.get(ip);

  if (!record) {
    rateLimitMap.set(ip, { count: 1, windowStart: now });
    return false;
  }

  // Reset window if expired
  if (now - record.windowStart > RATE_LIMIT_WINDOW_MS) {
    rateLimitMap.set(ip, { count: 1, windowStart: now });
    return false;
  }

  // Check if over limit
  if (record.count >= MAX_REQUESTS_PER_WINDOW) {
    return true;
  }

  // Increment count
  record.count++;
  return false;
}

// Sanitize input to prevent injection attacks
function sanitizeInput(str, maxLength = 50) {
  if (typeof str !== 'string') return '';
  // Remove any Discord markdown/mentions and limit length
  return str
    .replace(/@everyone/gi, '')
    .replace(/@here/gi, '')
    .replace(/<@[!&]?\d+>/g, '')
    .replace(/[<>]/g, '')
    .substring(0, maxLength)
    .trim();
}

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

  // Get client IP for rate limiting
  const clientIp = req.headers['x-forwarded-for']?.split(',')[0] ||
                   req.headers['x-real-ip'] ||
                   'unknown';

  // Check rate limit
  if (isRateLimited(clientIp)) {
    console.log(`Rate limited request from IP: ${clientIp}`);
    return res.status(429).json({ error: 'Too many requests. Please try again later.' });
  }

  try {
    const { userName, score, isNewHighScore, currentTopScore, apiKey } = req.body;

    // Validate API key
    const expectedApiKey = process.env.API_SECRET;
    if (!expectedApiKey) {
      console.error('API_SECRET environment variable not configured');
      return res.status(500).json({ error: 'Server configuration error' });
    }

    if (!apiKey || apiKey !== expectedApiKey) {
      console.log(`Unauthorized request attempt from IP: ${clientIp}`);
      return res.status(401).json({ error: 'Unauthorized' });
    }

    // Validate required fields
    if (!userName || !score) {
      return res.status(400).json({ error: 'Missing required fields' });
    }

    // Basic score sanity (non-negative number only; no upper limit)
    const scoreNum = parseInt(score, 10);
    if (isNaN(scoreNum) || scoreNum < 0) {
      return res.status(400).json({ error: 'Invalid score value' });
    }

    // Sanitize username
    const sanitizedUserName = sanitizeInput(userName, 20);
    if (sanitizedUserName.length < 1) {
      return res.status(400).json({ error: 'Invalid username' });
    }

    // Get Discord webhook URL from environment variable
    const webhookUrl = process.env.DISCORD_WEBHOOK_URL;
    if (!webhookUrl) {
      return res.status(500).json({ error: 'Discord webhook URL not configured' });
    }

    // Create message based on score type
    let message = `🐉 ${sanitizedUserName} has achieved a new high score of ${scoreNum}!!!`;

    // Send to Discord
    await axios.post(webhookUrl, {
      username: 'BabyLoongGame',
      content: message
    });

    console.log(`High score posted for ${sanitizedUserName}: ${scoreNum}`);
    return res.status(200).json({ success: true, message: 'Message sent to Discord' });
  } catch (error) {
    console.error('Error:', error.message);
    return res.status(500).json({ error: 'Failed to send message to Discord' });
  }
};
