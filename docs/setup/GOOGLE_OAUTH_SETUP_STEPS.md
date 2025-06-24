# Google OAuth Setup - Step by Step Fix

## 🚨 Current Error: "Access Blocked" Error 400: invalid_request

This error occurs because the Google OAuth client ID is not properly configured for your domain.

## 🔧 Step-by-Step Fix

### Step 1: Access Google Cloud Console

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Sign in with your Google account
3. Select your project (or create one if needed)

### Step 2: Navigate to OAuth Credentials

1. In the left sidebar, click **"APIs & Services"**
2. Click **"Credentials"**
3. Find your OAuth 2.0 client ID: `329249059487-5vouq4j4h6d60skumv9deilv62mnfdah.apps.googleusercontent.com`
4. Click the **pencil icon (Edit)** next to it

### Step 3: Configure Authorized Origins

In the edit form:

1. **Authorized JavaScript origins** - Add these URLs:
   ```
   http://localhost:4000
   http://localhost:3000
   ```

2. **Authorized redirect URIs** - Add these URLs:
   ```
   http://localhost:4000
   http://localhost:4000/
   http://localhost:4000/dashboard
   ```

3. Click **"Save"**

### Step 4: Verify OAuth Consent Screen

1. Go to **"APIs & Services"** → **"OAuth consent screen"**
2. Make sure your app is configured:
   - **User Type**: External (for development)
   - **App name**: "Ideas Matter" (or your preferred name)
   - **User support email**: Your email
   - **Developer contact information**: Your email

### Step 5: Enable Required APIs

1. Go to **"APIs & Services"** → **"Library"**
2. Search for and enable these APIs:
   - **Google+ API** (if not already enabled)
   - **Google Identity and Access Management (IAM) API**

## 🔄 Restart Services

After making these changes:

1. Stop both frontend and backend services
2. Restart them using: `.\start_dev.bat`
3. Clear your browser cache and cookies for localhost
4. Try the Google login again

## 🚨 Alternative: Use Bypass Login (Immediate Solution)

If you can't configure Google OAuth right now, use the bypass login:

1. Open http://localhost:4000
2. Look for the **"Development Mode"** section with bypass buttons
3. Click **"Login as User"** or **"Login as Admin"**
4. This will authenticate you without Google OAuth

## 🔍 Debug Steps

### Check Environment Variables

Open browser console and run:
```javascript
console.log('Google Client ID:', import.meta.env.VITE_GOOGLE_CLIENT_ID)
console.log('API Base URL:', import.meta.env.VITE_API_BASE_URL)
```

### Test Backend Connectivity

```bash
curl http://localhost:8000/health
```

### Check Network Tab

1. Open browser Developer Tools
2. Go to Network tab
3. Try Google login
4. Look for failed requests and their details

## 📋 Common Issues and Solutions

### Issue: "The given origin is not allowed for the given client ID"
**Solution**: Add `http://localhost:4000` to authorized origins in Google Cloud Console

### Issue: "Invalid client ID"
**Solution**: Verify the client ID in your .env file matches the one in Google Cloud Console

### Issue: "Redirect URI mismatch"
**Solution**: Add the correct redirect URIs in Google Cloud Console

### Issue: "OAuth consent screen not configured"
**Solution**: Configure the OAuth consent screen in Google Cloud Console

## 🎯 Quick Test

After configuration, test with this simple HTML file:

```html
<!DOCTYPE html>
<html>
<head>
    <title>Google OAuth Test</title>
    <script src="https://accounts.google.com/gsi/client" async defer></script>
</head>
<body>
    <h1>Google OAuth Test</h1>
    <div id="g_id_onload"
         data-client_id="329249059487-5vouq4j4h6d60skumv9deilv62mnfdah.apps.googleusercontent.com"
         data-callback="handleCredentialResponse">
    </div>
    <div class="g_id_signin" data-type="standard"></div>
    
    <script>
        function handleCredentialResponse(response) {
            console.log("Success! Token:", response.credential);
            document.getElementById('result').innerHTML = 'Success! Check console for token.';
        }
    </script>
    
    <div id="result"></div>
</body>
</html>
```

Save this as `test_google.html` and open it in your browser to test the Google OAuth configuration.

## 📞 Need Help?

If you're still having issues:

1. Check the browser console for specific error messages
2. Verify all URLs in Google Cloud Console are exactly as shown
3. Make sure there are no extra spaces or characters in the client ID
4. Try the bypass login as a temporary solution 