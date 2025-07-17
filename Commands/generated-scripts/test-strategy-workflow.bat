@echo off
echo Testing Strategy Workflow...
echo.
cd /d C:\Development\Jackson.Ideas

echo Running strategy workflow in preview mode...
gh workflow run create-strategy.yml -f vision_issue_number=1 -f timeframe_months=18 -f preview=true

echo.
echo Workflow dispatched! Check GitHub Actions for results:
echo https://github.com/bjackson071968/Jackson.Ideas/actions

pause