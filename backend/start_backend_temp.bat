@echo off 
echo Starting backend server... 
call venv\Scripts\activate.bat 
if errorlevel 1 ( 
    echo Failed to activate virtual environment 
    pause 
    exit /b 1 
) 
pip install -r requirements.txt 
if errorlevel 1 ( 
    echo Failed to install requirements 
    pause 
    exit /b 1 
) 
python main.py 
