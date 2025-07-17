Set-Location "C:\Development\Jackson.Ideas"
gh workflow run create-strategy.yml -f vision_issue_number=1 -f timeframe_months=18 -f preview=true
Read-Host "Press Enter to continue"