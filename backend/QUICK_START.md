# 🚀 Quick Start Guide

## One-Command Launch

```bash
cd backend
python quick_start.py
```

That's it! The script will:
- ✅ Create a virtual environment
- ✅ Install all dependencies
- ✅ Use SQLite (no database setup needed)
- ✅ Start the server on http://localhost:8000

## Manual Launch (Alternative)

```bash
cd backend
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate
pip install -r requirements.txt
python main.py
```

## 🌐 Access Points

- **API**: http://localhost:8000
- **Interactive Docs**: http://localhost:8000/docs
- **ReDoc**: http://localhost:8000/redoc

## 🎯 What's Included

- ✅ **FastAPI** backend with async support
- ✅ **SQLite** database (auto-created)
- ✅ **JWT Authentication** (pre-configured)
- ✅ **RESTful API** endpoints
- ✅ **AI Provider** abstraction layer
- ✅ **Modular architecture**

## 🔧 Quick Configuration

The `.env` file is pre-configured for quick launch. To add AI features:

```bash
# Edit .env and add:
OPENAI_API_KEY=your-key-here
CLAUDE_API_KEY=your-key-here
```

## 📝 Notes

- Uses SQLite instead of MySQL (no setup required)
- Redis dependency is disabled for quick launch
- Default JWT secret (change for production!)
- All data stored in `payroll_hr.db` file

## 🚨 Troubleshooting

If you encounter issues:

1. Ensure Python 3.8+ is installed
2. Try manual installation steps
3. Check firewall for port 8000
4. Delete `venv` folder and retry

## 🎉 Next Steps

1. Open http://localhost:8000/docs
2. Try the API endpoints
3. Review the architecture in `/ARCHITECTURE.md`
4. Check component breakdown in `/COMPONENTS.md`