# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Ideas Matter is an AI-powered idea development platform that transforms concepts into deployable code. The system conducts market research, creates business plans, generates technical architecture, and produces production-ready applications.

**Tech Stack:**
- Frontend: React 18 + TypeScript + Vite + Tailwind CSS
- Backend: FastAPI + SQLAlchemy + Multiple AI providers (OpenAI, Claude, Azure OpenAI)
- Database: MySQL/SQLite with Alembic migrations
- AI Integration: Multi-provider architecture with OpenAI, Claude, and LangChain support

## Development Commands

### Quick Start (Recommended)
```bash
# Backend (from backend/)
python quick_start.py

# Frontend (from frontend/)
npm install
npm run dev
```

### Manual Setup
```bash
# Backend
cd backend
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate
pip install -r requirements.txt
python main.py

# Frontend
cd frontend
npm install
npm run dev
```

### Development Scripts
- `start_dev.bat` / `start_dev.ps1` - Intelligent scripts that stop existing services, start fresh, and open browser
- These scripts handle all setup automatically including dependency installation

Additional scripts:
- `scripts/test/run_tests.py` - Unified test runner
- `scripts/setup/quick_setup.py` - One-command project setup

Backend runs on http://localhost:8000 (API docs at /docs)
Frontend runs on http://localhost:4000

### Testing
```bash
# Backend tests
cd backend
pytest tests/

# Frontend
cd frontend
npm run lint       # ESLint checking
npm run build     # Production build
npm run preview   # Preview production build
```

### Database Management
```bash
# Create migration
cd backend
alembic revision --autogenerate -m "description"

# Apply migrations
alembic upgrade head
```

## Architecture Key Points

### AI Orchestration Layer
The system uses a provider abstraction pattern in `backend/app/ai/` that supports multiple AI services:
- OpenAI adapter (`providers/openai_provider.py`)
- Modular agent system in `agents/`
- Template-driven prompt management

### Backend Structure
- `app/api/v1/endpoints/` - API route handlers
- `app/services/` - Business logic layer including AI orchestration
- `app/models/` - SQLAlchemy database models
- `app/schemas/` - Pydantic request/response schemas
- `app/core/` - Configuration and database setup

### Frontend Structure
- `src/components/` - Reusable React components
- `src/pages/` - Page-level components
- `src/services/` - API client code
- `src/contexts/` - React context providers (AuthContext)

### Configuration
- Backend config in `backend/app/core/config.py` uses Pydantic Settings
- Supports quick launch mode with SQLite (QUICK_LAUNCH=True)
- AI provider keys configured via environment variables
- Google OAuth integration available

### Environment Variables
Key environment variables for backend:
- `JWT_SECRET_KEY` - Generate: `python -c "import secrets; print(secrets.token_urlsafe(32))"`
- `ENCRYPTION_KEY` - Generate: `python -c "from cryptography.fernet import Fernet; print(Fernet.generate_key().decode())"`
- `ADMIN_EMAILS` - Comma-separated list of admin user emails
- `RATE_LIMIT_ENABLED` / `RATE_LIMIT_PER_MINUTE` - API rate limiting
- Multiple AI provider keys (OPENAI_API_KEY, ANTHROPIC_API_KEY, etc.)

## Important Notes

- The system supports both MySQL (production) and SQLite (development) databases
- Quick launch mode bypasses Redis dependency for rapid development
- Multi-tenant architecture with role-based access control
- Comprehensive error handling and logging with Loguru
- Uses Alembic for database migrations
- Authentication via JWT with Google OAuth support
- Multiple requirements files available: requirements-core.txt, requirements-minimal.txt, requirements-windows.txt
- Admin access granted to emails containing "admin@" or "systemadmin@"
- Admin dashboard available at `/admin` route

## Scripts Organization

Development scripts organized in structured directories:

**Main Scripts:**
- `start_dev.bat` / `start_dev.ps1` - Single intelligent scripts that handle everything: stop existing services, install dependencies, start servers, open browser

**Additional Scripts:**
- `scripts/setup/` - Installation and setup scripts
- `scripts/test/` - Test runners and utilities (HTML and Python tests)
- `scripts/utils/` - Various utility scripts
- `backend/scripts/auth/` - Authentication server variants
- `backend/scripts/db/` - Database management scripts
- `backend/scripts/test/` - Backend-specific test scripts
- `backend/scripts/setup/` - Alternative server configurations

**Documentation:**
- `docs/` - Organized documentation (architecture, troubleshooting, setup, fixes)
- `scripts/README.md` - Script usage instructions
- `docs/README.md` - Documentation index

## Authentication Setup

### Production Authentication (Recommended)
Use the production-ready authentication server with full JWT and database support:
```bash
cd backend
python3 scripts/auth/production_server.py
```

**Features:**
- JWT access and refresh tokens with proper expiration
- SQLite database with persistent user storage
- Password hashing and secure authentication
- Role-based access control (user, admin, system_admin)
- User registration and login endpoints
- Google OAuth integration ready
- Comprehensive API endpoints

**Endpoints:**
- `POST /api/v1/auth/register` - User registration
- `POST /api/v1/auth/login` - Email/password login
- `POST /api/v1/auth/google` - Google OAuth login
- `GET /api/v1/auth/me` - Get current user info
- `POST /api/v1/auth/bypass` - Development bypass login
- `GET /health` - Health check

### Development Quick Start
For minimal dependency development, use:
```bash
cd backend
python3 scripts/auth/auth_server_with_google.py
```

### Production Configuration
Copy `.env.production` to `.env` and configure:
- Database credentials (MySQL/PostgreSQL for production)
- JWT secret keys (minimum 32 characters)
- Google OAuth client credentials
- CORS origins for production domain

The frontend has bypass login buttons in development mode for quick access without Google OAuth configuration.