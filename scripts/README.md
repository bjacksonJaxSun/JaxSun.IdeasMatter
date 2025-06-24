# Scripts Directory

This directory contains all development, test, and utility scripts for the Ideas Matter project.

## Directory Structure

```
root/
├── start_dev.bat      # Single intelligent startup script (Windows)
├── start_dev.ps1      # Single intelligent startup script (PowerShell)
├── scripts/           # Development and utility scripts
│   ├── setup/        # Setup and installation scripts
│   │   ├── quick_setup.py     # One-command setup script
│   │   └── install_deps.bat   # Dependency installation
│   ├── test/         # Test scripts and utilities
│   │   ├── run_tests.py       # Unified test runner
│   │   ├── test_auth.py       # Comprehensive auth testing
│   │   ├── html/              # HTML test files
│   │   └── python/            # Python test scripts
│   └── utils/        # Utility scripts
│       ├── check_services.bat # Service status checker
│       └── fix_explorer.bat   # Windows Explorer fix
└── backend/scripts/   # Backend-specific scripts
    ├── auth/         # Authentication server variants
    ├── db/           # Database management scripts
    ├── test/         # Backend test scripts
    └── setup/        # Alternative server setups
```

## Quick Start

### First Time Setup
```bash
# Run the quick setup script
python scripts/setup/quick_setup.py

# Or manually install dependencies
scripts/setup/install_deps.bat
```

### Start Development
```bash
# Windows Command Prompt
start_dev.bat

# PowerShell
start_dev.ps1
```

These scripts automatically:
- Stop any existing services
- Install/update dependencies as needed
- Start backend and frontend servers
- Open browser to the application

To stop services, just run the same script again or close the server windows.

### Run Tests
```bash
# Run all tests
python scripts/test/run_tests.py --all

# Run specific test suite
python scripts/test/run_tests.py auth
python scripts/test/run_tests.py swot
python scripts/test/run_tests.py market

# Run with verbose output
python scripts/test/run_tests.py -v

# Test authentication flow
python scripts/test/test_auth.py
```

## Backend Scripts

Additional scripts are located in `backend/scripts/`:

- `backend/scripts/auth/` - Authentication server variants
- `backend/scripts/db/` - Database management scripts
- `backend/scripts/test/` - Backend-specific test scripts

## Notes

- All scripts assume they are run from the project root directory
- The development scripts will automatically install dependencies if needed
- Backend runs on http://localhost:8000
- Frontend runs on http://localhost:4000
- API documentation available at http://localhost:8000/docs