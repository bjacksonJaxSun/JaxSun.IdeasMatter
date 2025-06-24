# Google Authentication Setup Guide

## 🔐 Setting Up Google OAuth for Ideas Matter

### Prerequisites
1. A Google account
2. Access to Google Cloud Console

### Step 1: Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click "Select a project" → "New Project"
3. Name your project (e.g., "Ideas Matter")
4. Click "Create"

### Step 2: Enable Google+ API

1. In the Google Cloud Console, go to "APIs & Services" → "Library"
2. Search for "Google+ API"
3. Click on it and press "Enable"

### Step 3: Create OAuth 2.0 Credentials

1. Go to "APIs & Services" → "Credentials"
2. Click "Create Credentials" → "OAuth client ID"
3. If prompted, configure the OAuth consent screen:
   - Choose "External" user type
   - Fill in the application name: "Ideas Matter"
   - Add your email as support email
   - Add authorized domains (for production)
   - Save and continue

4. Back in Create OAuth client ID:
   - Application type: "Web application"
   - Name: "Ideas Matter Web Client"
   - Authorized JavaScript origins:
     - `http://localhost:4000` (for development)
     - Your production URL (when deployed)
   - Authorized redirect URIs:
     - `http://localhost:4000` (for development)
     - Your production URL (when deployed)
   - Click "Create"

### Step 4: Copy Your Client ID

1. After creation, you'll see your Client ID
2. Copy the Client ID (looks like: `123456789012-abcdefghijklmnop.apps.googleusercontent.com`)

### Step 5: Configure Your Application

1. Open `/frontend/.env` file
2. Replace the placeholder with your actual Client ID:
   ```
   VITE_GOOGLE_CLIENT_ID=your-actual-client-id-here.apps.googleusercontent.com
   ```

### Step 6: Test the Integration

1. Start your frontend:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

2. Go to http://localhost:4000
3. Click "Login" or "Start Building"
4. Click the "Google" button
5. You should see Google's login screen

### 🎉 Success!

Once configured, users can:
- Sign up with Google
- Login with Google
- Their Google profile picture and name will be used

### Troubleshooting

**"Access blocked" error:**
- Make sure you've added `http://localhost:4000` to authorized origins
- Check that the consent screen is configured

**"Invalid client ID" error:**
- Verify the client ID is copied correctly
- Ensure there are no extra spaces or characters

**Login popup doesn't appear:**
- Check browser console for errors
- Ensure popups aren't blocked
- Verify the client ID is set in `.env`

### Production Deployment

When deploying to production:
1. Add your production URL to authorized origins
2. Update the redirect URIs
3. Set the production client ID in your environment variables
4. Consider restricting API key usage to your domain

### Security Notes

- Never commit your Client ID to public repositories (though it's less sensitive than secrets)
- For production, implement server-side token validation
- Use HTTPS in production
- Implement proper session management