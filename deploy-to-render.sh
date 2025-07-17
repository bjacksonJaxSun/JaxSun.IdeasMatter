#!/bin/bash

# Deploy to Render script
echo "🚀 Deploying Jackson.Ideas to Render..."

# Export GitHub CLI path
export PATH="$HOME/.local/bin:$PATH"

# Check if we're authenticated
if ! gh auth status > /dev/null 2>&1; then
    echo "❌ GitHub CLI not authenticated. Please run: gh auth login"
    exit 1
fi

# Check if we have uncommitted changes
if ! git diff --quiet || ! git diff --cached --quiet; then
    echo "📝 Committing current changes..."
    git add .
    git commit -m "Prepare for Render deployment - consolidate UI and fix navigation

🔧 Changes Made:
- Consolidated multiple review and submit pages into single streamlined flow
- Fixed post-submission navigation to stay on progress tracking page
- Removed duplicate strategy selection components
- Updated form flow from 5 steps to 4 steps
- Fixed template selection to use IndustryTemplate instead of ResearchStrategyModel

🎯 Result:
- Single, streamlined submission flow
- Immediate progress tracking after submission
- No navigation disruption during research process
- Better user experience with fewer steps

🤖 Generated with [Claude Code](https://claude.ai/code)

Co-Authored-By: Claude <noreply@anthropic.com>"
fi

# Push to GitHub
echo "📤 Pushing changes to GitHub..."
git push origin master

# Check if push was successful
if [ $? -eq 0 ]; then
    echo "✅ Successfully pushed to GitHub"
    echo "🔄 Render will automatically detect the changes and start deployment"
    echo ""
    echo "📋 Deployment Status:"
    echo "- Repository: https://github.com/bjackson071968/Jackson.Ideas"
    echo "- Branch: master"
    echo "- Render should detect changes and start building automatically"
    echo ""
    echo "🔗 Next Steps:"
    echo "1. Monitor deployment progress on Render dashboard"
    echo "2. Check application logs for any issues"
    echo "3. Test the deployed application"
else
    echo "❌ Failed to push to GitHub"
    exit 1
fi

echo ""
echo "✨ Deployment process initiated successfully!"