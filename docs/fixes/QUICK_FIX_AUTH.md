# Quick Fix for Authentication After start_dev.bat

## Current Status
- ✅ Backend auth server is running on port 8000
- ❌ Frontend is not running on port 4000

## Quick Fix Steps

### Option 1: Manual Start (Recommended)

1. **Keep the auth server running** (it's already running)

2. **Start frontend manually:**
```bash
cd frontend
npm run dev
```

### Option 2: Use PowerShell Commands

Open PowerShell as Administrator and run:

```powershell
# Check what's running
netstat -an | findstr :8000
netstat -an | findstr :4000

# Start frontend only (backend is already running)
cd C:\Development\Jackson.Ideas\frontend
npm run dev
```

### Option 3: Kill Everything and Restart

1. **Stop all services:**
```bash
# In Windows Command Prompt
taskkill /F /IM node.exe
taskkill /F /IM python.exe
```

2. **Use the fixed startup script:**
```bash
cd C:\Development\Jackson.Ideas
start_dev_fixed.bat
```

## Why This Happens

The original `start_dev.bat` tries to run `python main.py` which requires uvicorn and other dependencies. When it fails, the frontend might not start properly either.

Our auth server (`auth_server_with_google.py`) is a lightweight replacement that doesn't need any dependencies and provides all authentication endpoints.

## Verification

Once both services are running, test authentication:

1. Open http://localhost:4000
2. Click "Login as User" or "Login as Admin"
3. You should be redirected to the dashboard

Or test the API directly:
```bash
curl -X POST http://localhost:8000/api/v1/auth/bypass -H "Content-Type: application/json" -d "{\"role\":\"user\"}"
```

## Permanent Fix

Replace the backend startup in `start_dev.bat`:

Change line 13 from:
```batch
start "Ideas Matter Backend" cmd /k "venv\Scripts\activate.bat && python main.py"
```

To:
```batch
start "Ideas Matter Backend" cmd /k "python auth_server_with_google.py"
```

This will use our enhanced auth server that doesn't require dependencies.