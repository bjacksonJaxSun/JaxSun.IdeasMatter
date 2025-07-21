# Golden Rule Enforcement Scripts

This directory contains scripts that implement the **Golden Rule** for the Ideas Matter project:

> **Golden Rule**: Before completing a set of tasks, run build, fix, build cycle until all errors are resolved. Next, launch the application and capture the browser content. If there are issues, fix and repeat until all issues are fixed and the layout is what is expected. Finally, ensure that the session is closed.

## Scripts Available

### 1. `golden-rule.sh` (Linux/macOS)
Bash script for Unix-like systems.

**Usage:**
```bash
# Make executable (first time only)
chmod +x scripts/golden-rule.sh

# Run with default project (Jackson.Ideas.Mock)
./scripts/golden-rule.sh

# Run with specific project
./scripts/golden-rule.sh Jackson.Ideas.Api

# Keep application running for manual testing
./scripts/golden-rule.sh Jackson.Ideas.Mock --keep-running
```

### 2. `golden-rule.ps1` (Windows PowerShell)
PowerShell script for Windows systems.

**Usage:**
```powershell
# Run with default project
.\scripts\golden-rule.ps1

# Run with specific project
.\scripts\golden-rule.ps1 -ProjectName "Jackson.Ideas.Mock"

# Keep application running
.\scripts\golden-rule.ps1 -ProjectName "Jackson.Ideas.Mock" -KeepRunning
```

### 3. `golden-rule.bat` (Windows Batch)
Simple batch wrapper for easier Windows execution.

**Usage:**
```cmd
# Run with default project
scripts\golden-rule.bat

# Run with specific project
scripts\golden-rule.bat Jackson.Ideas.Mock

# Keep application running
scripts\golden-rule.bat Jackson.Ideas.Mock --keep-running

# Show help
scripts\golden-rule.bat --help
```

## What the Scripts Do

### 1. Environment Validation
- ✅ Checks for .NET CLI availability
- ✅ Validates project structure
- ✅ Ensures project files exist

### 2. Build Process (with Auto-Fixing)
- 🔨 Cleans solution before building
- 🔨 Attempts build with up to 3 retries
- 🔧 Auto-fixes common issues:
  - Restores NuGet packages
  - Clears NuGet cache
  - Removes bin/obj directories
- 📋 Logs all build output for debugging

### 3. Application Launch
- 🚀 Starts the application in background
- ⏱️ Waits for HTTP endpoints to respond
- 🔄 Retries launch up to 3 times if needed
- 📊 Monitors process health

### 4. Layout Validation
- 🌐 Tests critical pages (/, /login, /submit-idea, /dashboard)
- 📸 Captures screenshots using headless browser
- 🔍 Validates static resources (CSS, JavaScript)
- 🧪 Tests API endpoints if available

### 5. Reporting & Cleanup
- 📝 Generates comprehensive validation report
- 📸 Saves screenshots with timestamps
- 📋 Preserves all logs for troubleshooting
- 🧹 Properly terminates all processes
- 🛡️ Handles cleanup even if script is interrupted

## Output Locations

### Logs Directory: `logs/`
- `golden-rule.log` - Main script execution log
- `build-*.log` - Build attempt logs
- `app-*.log` - Application runtime logs
- `golden-rule-report-*.md` - Validation reports

### Screenshots Directory: `screenshots/`
- `golden-rule-*_root.png` - Homepage screenshot
- `golden-rule-*_login.png` - Login page screenshot
- `golden-rule-*_submit-idea.png` - Idea submission page
- `golden-rule-*_dashboard.png` - Dashboard page

## Error Handling

The scripts include comprehensive error handling:

### Build Errors
- **Auto-retry**: Up to 3 build attempts
- **Auto-fix**: Common NuGet and dependency issues
- **Logging**: Detailed error output saved to logs
- **Exit codes**: Non-zero exit on failure

### Launch Errors
- **Timeout handling**: Waits up to 30 seconds for startup
- **Port conflicts**: Detects and handles port issues
- **Process management**: Proper cleanup of failed processes

### Browser/Screenshot Errors
- **Graceful degradation**: Continues if browser unavailable
- **Multiple browser support**: Chrome, Chromium, Firefox
- **Timeout protection**: Prevents hanging on browser issues

## Prerequisites

### For All Scripts
- **.NET SDK**: Version 8.0 or later
- **Network access**: For NuGet package restoration

### For Screenshots (Optional)
- **Google Chrome** or **Chromium**: For headless screenshots
- **Display server**: X11 on Linux (for headless mode)

### Platform-Specific

#### Linux/macOS (`golden-rule.sh`)
```bash
# Install dependencies (Ubuntu/Debian)
sudo apt-get update
sudo apt-get install curl wget google-chrome-stable

# Install dependencies (macOS with Homebrew)
brew install --cask google-chrome
```

#### Windows (`golden-rule.ps1` / `golden-rule.bat`)
```powershell
# Install Google Chrome (if not already installed)
# Download from: https://www.google.com/chrome/
```

## Integration with Development Workflow

### Git Hooks Integration
Add to `.git/hooks/pre-commit`:
```bash
#!/bin/bash
./scripts/golden-rule.sh Jackson.Ideas.Mock
```

### CI/CD Integration
Add to GitHub Actions workflow:
```yaml
- name: Golden Rule Enforcement
  run: ./scripts/golden-rule.sh Jackson.Ideas.Mock
```

### IDE Integration
**VS Code Task** (`.vscode/tasks.json`):
```json
{
    "label": "Golden Rule",
    "type": "shell",
    "command": "./scripts/golden-rule.sh",
    "args": ["Jackson.Ideas.Mock"],
    "group": "build",
    "presentation": {
        "echo": true,
        "reveal": "always",
        "panel": "new"
    }
}
```

## Troubleshooting

### Common Issues

#### "dotnet not found"
**Solution**: Install .NET SDK and ensure it's in PATH
```bash
# Download from: https://dotnet.microsoft.com/download
# Or use package manager
```

#### "Application failed to start"
**Possible causes**:
- Port 5000/5001 already in use
- Missing dependencies
- Configuration errors

**Solution**: Check logs in `logs/app-*.log`

#### "No browser for screenshots"
**Solution**: Install Google Chrome or Chromium
```bash
# Ubuntu/Debian
sudo apt-get install google-chrome-stable

# Windows: Download from google.com/chrome
```

#### "Permission denied"
**Linux/macOS Solution**:
```bash
chmod +x scripts/golden-rule.sh
```

**Windows Solution**: Run as Administrator or adjust PowerShell execution policy:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Debug Mode

For detailed debugging, check the generated logs:

```bash
# View main log
cat logs/golden-rule.log

# View latest build log
ls -la logs/build-*.log | tail -1 | awk '{print $9}' | xargs cat

# View latest app log
ls -la logs/app-*.log | tail -1 | awk '{print $9}' | xargs cat
```

## Script Customization

### Timeout Adjustments
Edit the configuration variables at the top of each script:
```bash
# In golden-rule.sh
LAUNCH_TIMEOUT=30          # Increase for slower systems
BROWSER_CHECK_TIMEOUT=60   # Increase for complex pages
MAX_BUILD_ATTEMPTS=3       # Increase for flaky builds
```

### Adding Custom Validation
Extend the `validate_critical_functions()` or `Test-CriticalFunctions` function:
```bash
# Test custom endpoint
curl -s "http://localhost:5000/api/custom" || echo "Custom API failed"
```

### Custom Screenshots
Add pages to the `pages` array:
```bash
pages=(
    "/"
    "/login"
    "/submit-idea"
    "/dashboard"
    "/custom-page"  # Add your page here
)
```

---

**Golden Rule Status**: 🎯 Scripts ready for enforcement!

*These scripts ensure your application meets the highest quality standards before task completion.*