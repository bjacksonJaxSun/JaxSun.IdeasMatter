# Complete Setup Guide for GitHub Automation

## Prerequisites Installation

### 1. Install Git for Windows
Download and install from: https://git-scm.com/download/win

During installation:
- Choose "Git from the command line and also from 3rd-party software"
- Keep all other defaults

### 2. Verify Git Installation
Open a new Command Prompt and run:
```cmd
git --version
```

### 3. Configure Git (if not already done)
```cmd
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

### 4. Authenticate GitHub CLI
Once Git is installed, authenticate gh:
```cmd
gh auth login
```

Choose:
- GitHub.com
- HTTPS
- Authenticate with a web browser

## Alternative: Use GitHub Web Interface

While setting up the CLI, you can create the vision using the GitHub web interface:

1. Go to: https://github.com/bjackson071968/Jackson.Ideas/actions
2. Click on "Create Product Vision" workflow
3. Click "Run workflow"
4. Fill in:
   - Product name: `Ideas Matter`
   - Vision file path: `Commands/docs/visions/ideas-matter/vision.md`
   - Preview: `false`
5. Click "Run workflow"

## What Has Been Completed

✅ Vision document created and validated
✅ Committed and pushed to GitHub
✅ Ready to be processed by workflow

The vision file is at: `Commands/docs/visions/ideas-matter/vision.md`

## Quick Test After Git Installation

Once Git is installed, test the complete automation:
```cmd
cd C:\Development\Jackson.Ideas\Commands\generated-scripts
run-workflow.bat
```

Or manually:
```cmd
cd C:\Development\Jackson.Ideas
gh workflow run create-vision.yml -f product_name="Ideas Matter" -f vision_file_path="Commands/docs/visions/ideas-matter/vision.md" -f preview=false
```