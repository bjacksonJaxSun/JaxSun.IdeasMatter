# Google OAuth Fix Guide

## 🔧 Issues Identified

1. **Origin not allowed for client ID** - Google OAuth configuration issue
2. **Timeout errors** - Backend API not responding properly  
3. **CORS/Cross-Origin issues** - Policy blocking postMessage calls
4. **Missing environment variables** - Frontend and backend not properly configured

## ✅ Fixes Applied

### 1. Environment Configuration

**Frontend (.env file created):**
```
VITE_GOOGLE_CLIENT_ID=329249059487-5vouq4j4h6d60skumv9deilv62mnfdah.apps.googleusercontent.com
VITE_API_BASE_URL=http://localhost:8000/api/v1
VITE_DEV_MODE=true
```

**Backend (.env file created):**
```
GOOGLE_CLIENT_ID=329249059487-5vouq4j4h6d60skumv9deilv62mnfdah.apps.googleusercontent.com
```

### 2. CORS Configuration Updated

Updated `backend/config.json` to include `localhost:4000`:
```json
"cors_origins": ["http://localhost:3000", "http://localhost:4000"]
```

### 3. API Endpoint Fixes

Updated `frontend/src/contexts/AuthContext.tsx`:
- Fixed axios base URL configuration
- Removed duplicate `/api/v1` prefixes from API calls
- Added proper TypeScript types for environment variables

### 4. TypeScript Configuration

Created `frontend/src/vite-env.d.ts` to fix import.meta.env TypeScript errors.

## 🔧 Additional Steps Required

### 1. Google Cloud Console Configuration

You need to configure your Google OAuth client ID properly:

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Navigate to "APIs & Services" → "Credentials"
3. Find your OAuth 2.0 client ID: `329249059487-5vouq4j4h6d60skumv9deilv62mnfdah.apps.googleusercontent.com`
4. Click "Edit" (pencil icon)
5. Under "Authorized JavaScript origins", add:
   - `http://localhost:4000`
   - `http://localhost:3000` (if needed)
6. Under "Authorized redirect URIs", add:
   - `http://localhost:4000`
   - `http://localhost:4000/`
7. Click "Save"

### 2. Restart Services

After making these changes:

1. Stop both frontend and backend services
2. Restart the backend: `cd backend && python main.py`
3. Restart the frontend: `cd frontend && npm run dev`

### 3. Test the Fix

1. Open http://localhost:4000
2. Try the Google login button
3. Check browser console for any remaining errors

## 🚨 If Issues Persist

### Alternative: Use Bypass Login

If Google OAuth still doesn't work, you can use the bypass login feature:

1. The bypass login buttons should appear in development mode
2. Click "Login as User" or "Login as Admin"
3. This will authenticate you without Google OAuth

### Debug Steps

1. **Check browser console** for specific error messages
2. **Verify environment variables** are loaded:
   ```javascript
   console.log('Google Client ID:', import.meta.env.VITE_GOOGLE_CLIENT_ID)
   ```
3. **Test backend connectivity**:
   ```bash
   curl http://localhost:8000/health
   ```
4. **Check CORS headers** in browser network tab

## 📝 Notes

- The Google OAuth client ID in the test file matches the one in the .env files
- CORS is configured to allow all origins in debug mode
- The backend has proper Google OAuth verification implemented
- Mock authentication is available as a fallback

## 🔒 Security Note

For production deployment:
- Use HTTPS URLs in Google Cloud Console
- Restrict authorized origins to your production domain
- Implement proper session management
- Use environment variables for sensitive configuration 