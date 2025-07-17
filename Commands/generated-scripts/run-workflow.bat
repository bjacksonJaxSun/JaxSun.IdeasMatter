@echo off
cd /d C:\Development\Jackson.Ideas
gh workflow run create-strategy.yml --inputs vision_issue_number=1 timeframe_months=18 preview=true
pause
