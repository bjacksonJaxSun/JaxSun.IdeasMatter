# WSL/Windows Path Issue Fix

## Problem
Error: `No Python at '"/usr/bin\python.exe'`

This error occurs when Windows batch files get confused between WSL (Windows Subsystem for Linux) paths and native Windows paths.

## Root Cause
- WSL and Windows share the same file system but use different path conventions
- WSL uses Unix-style paths (`/usr/bin/python`)
- Windows uses Windows-style paths (`C:\Python\python.exe`)
- Git Bash or other Unix-like shells can cause path confusion in batch files

## Solutions

### Option 1: Use the Fixed Script (Recommended)
```bash
# Use the fixed version that explicitly handles Windows paths
start_dev.bat  # This now calls scripts/dev/start_dev_fixed.bat
```

### Option 2: Check Your Environment
```bash
# Check which Python is being used
where python
python --version

# Make sure you're using Windows Command Prompt, not Git Bash or WSL
# Open Command Prompt (cmd.exe) and run from there
```

### Option 3: Manual Steps
If the automated scripts still fail:

```bash
# Backend
cd backend
python -m venv venv
call venv\Scripts\activate.bat
pip install -r requirements.txt
python main.py

# Frontend (in another command prompt)
cd frontend
npm install
npm run dev
```

## Key Changes Made

### start_dev_fixed.bat
1. **Explicit Path Variables**: Sets clear Windows path variables
2. **Better Error Handling**: Checks each step and reports failures
3. **Temporary Batch File**: Creates a separate batch file to avoid command line parsing issues
4. **Full Path Display**: Shows exactly which directories are being used

### Why This Fixes It
- Avoids complex command chaining in `start` command
- Uses explicit Windows path separators (`\` not `/`)
- Calls `activate.bat` explicitly instead of just `activate`
- Separates each command into individual steps for better error tracking

## Prevention
To avoid this issue in the future:
1. Always use Windows Command Prompt (`cmd.exe`) for batch files
2. Avoid running batch files from Git Bash or WSL terminals
3. Use explicit Windows paths in batch files
4. Test scripts in clean Windows Command Prompt sessions

## Testing the Fix
```bash
# From Windows Command Prompt (not Git Bash or PowerShell)
cd C:\Development\Jackson.Ideas
start_dev.bat

# Should now work without the /usr/bin error
```