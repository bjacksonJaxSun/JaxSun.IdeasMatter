# Ideas Matter - Development Guide

## Quick Start

### Windows Users

**Option 1: Batch Scripts (Recommended)**
```bash
# Start both frontend and backend
.\start_dev.bat

# Stop all services  
.\stop_dev.bat
```

**Option 2: PowerShell Scripts**
```powershell
# Start both frontend and backend
.\start_dev.ps1

# Stop all services
.\stop_dev.ps1
```

### Manual Start (If scripts don't work)

**Backend:**
```powershell
cd backend
.\venv\Scripts\Activate.ps1
python main.py
```

**Frontend:**
```powershell
cd frontend
npm run dev
```

## Service URLs

- **Frontend**: http://localhost:4000
- **Backend API**: http://localhost:8000  
- **API Documentation**: http://localhost:8000/docs
- **API Schema**: http://localhost:8000/redoc

## Development Features

### Backend (FastAPI)
- ✅ Real JWT authentication system
- ✅ User registration and login
- ✅ Google OAuth integration ready
- ✅ Research session management
- ✅ AI provider integration (OpenAI, Claude, Gemini)
- ✅ Encrypted API key storage
- ✅ SQLite database (quick launch mode)
- ✅ Real-time idea development tools

### Frontend (React + TypeScript)
- ✅ Modern React with TypeScript
- ✅ Authentication context
- ✅ Protected routes
- ✅ Research dashboard
- ✅ Idea management interface
- ✅ Real API integration

## Configuration

### Environment Variables (.env)
The backend uses these key settings:
- `QUICK_LAUNCH=true` - Uses SQLite instead of MySQL
- `DEBUG=true` - Development mode
- `JWT_SECRET_KEY` - Auto-generated
- `ENCRYPTION_KEY` - Auto-generated for API key storage

### Database
Currently using SQLite in quick launch mode. The database file `ideas_matter.db` will be created automatically.

## Troubleshooting

### Common Issues

**Port Already in Use:**
- Backend (8000): Check if another FastAPI/Python service is running
- Frontend (3000): Check if another React app is running
- Use `netstat -ano | findstr :8000` to find processes using the port

**Python Virtual Environment:**
- Make sure you're in the `backend` directory
- Activate with `.\venv\Scripts\Activate.ps1`
- Reinstall dependencies if needed: `pip install -r requirements-fixed.txt`

**Node.js Dependencies:**
- Run `npm install` in the frontend directory
- Clear cache with `npm cache clean --force` if needed

**PowerShell Execution Policy:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Getting Help

- Check the API documentation at http://localhost:8000/docs
- View logs in the terminal windows
- For issues, check the GitHub repository or documentation

## Next Steps

1. **AI Integration**: Add your API keys for OpenAI, Claude, or Gemini in the admin panel
2. **Database**: Switch to MySQL/PostgreSQL for production
3. **Authentication**: Configure Google OAuth credentials
4. **Deployment**: Use Docker containers for production deployment