@echo off
echo Testing Strategy Workflow (Fixed)...
echo.
cd /d C:\Development\Jackson.Ideas

echo Method 1: Try with workflow file name...
gh workflow run .github/workflows/create-strategy.yml -f vision_issue_number=1 -f timeframe_months=18 -f preview=true

echo.
echo Method 2: Try with workflow name...
gh workflow run "Create Vision Strategy" -f vision_issue_number=1 -f timeframe_months=18 -f preview=true

echo.
echo Method 3: List available workflows...
gh workflow list

echo.
echo Check GitHub Actions for results:
echo https://github.com/bjackson071968/Jackson.Ideas/actions

pause