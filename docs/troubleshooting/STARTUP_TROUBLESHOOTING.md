# Startup Troubleshooting Guide

## Issues Found and Solutions

### 1. Python Path Error: "No Python at '/usr/bin\python.exe'"
This error indicates WSL (Windows Subsystem for Linux) path confusion. This happens when Git Bash or WSL paths get mixed with Windows paths.

**Solution**: Use the native Windows Python installation.

### 2. Backend Not Starting on Port 8000
The production_server.py was moved to `backend/scripts/auth/` but the startup script was still looking for it in the root backend directory.

**Fixed**: Updated the path in start_dev.bat to use `python scripts\auth\production_server.py`

## Recommended Actions

### Option 1: Use the Debug Script (Recommended for Troubleshooting)
```bash
scripts\dev\start_dev_debug.bat
```
This will show detailed output about what's happening during startup.

### Option 2: Use the Simple Script (Uses main.py directly)
```bash
scripts\dev\start_dev_simple.bat
```
This bypasses the production server and uses the standard FastAPI entry point.

### Option 3: Start Services Manually

**Backend:**
```bash
cd backend
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
python main.py
```

**Frontend:**
```bash
cd frontend
npm install
npm run dev
```

## Common Issues and Fixes

### 1. Virtual Environment Issues
If you see venv-related errors, delete the venv folder and let it recreate:
```bash
cd backend
rmdir /s venv
```

### 2. Port Already in Use
If port 8000 or 4000 is already in use:
```bash
# Check what's using the ports
netstat -ano | findstr :8000
netstat -ano | findstr :4000

# Kill the process using the PID from above
taskkill /F /PID [PID_NUMBER]
```

### 3. Missing Dependencies
If you get import errors, ensure all requirements are installed:
```bash
cd backend
venv\Scripts\activate
pip install -r requirements.txt
```

### 4. Environment Variables
Make sure you have a `.env` file in the backend directory. If not:
```bash
cd backend
copy .env.example .env
```

## Quick Check Script
Run this to check your environment:
```bash
scripts\utils\check_services.bat
```