# Setting up GitHub CLI for Full Automation

The GitHub CLI is installed but needs to be authenticated. Here's how to set it up:

## Quick Setup (One-time only)

1. **Open Windows Command Prompt or PowerShell** (not WSL)

2. **Authenticate GitHub CLI**:
   ```cmd
   gh auth login
   ```
   
   When prompted:
   - Choose: GitHub.com
   - Choose: HTTPS
   - Authenticate with: a web browser
   - Follow the browser prompts

3. **Test authentication**:
   ```cmd
   gh auth status
   ```

## Running the Vision Creation

Once authenticated, you can either:

### Option A: Use the generated script
Double-click: `C:\Development\Jackson.Ideas\Commands\generated-scripts\run-workflow.bat`

### Option B: Run from command line
```cmd
cd C:\Development\Jackson.Ideas
gh workflow run create-vision.yml -f product_name="Ideas Matter" -f vision_file_path="Commands/docs/visions/ideas-matter/vision.md" -f preview=false
```

### Option C: Set up token for WSL automation
1. Create a GitHub Personal Access Token:
   - Go to https://github.com/settings/tokens
   - Generate new token (classic)
   - Select scopes: `repo`, `workflow`
   
2. Add to WSL environment:
   ```bash
   echo 'export GH_TOKEN="your-token-here"' >> ~/.bashrc
   source ~/.bashrc
   ```

## What the workflow will create:
- ✅ GitHub Issue titled "Vision: Ideas Matter" (pinned)
- ✅ GitHub Project for tracking
- ✅ All necessary labels
- ✅ Links and references

## Verification
After running, check:
- https://github.com/bjackson071968/Jackson.Ideas/issues (for the Vision issue)
- https://github.com/bjackson071968/Jackson.Ideas/projects (for the Project board)