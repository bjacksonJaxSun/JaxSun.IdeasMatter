# Ideas Matter - Documentation

This directory contains comprehensive documentation for the Ideas Matter project.

## Directory Structure

```
docs/
├── README.md                 # This file
├── README_DEV.md            # Development guide
├── architecture/            # Architecture and design docs
│   ├── ADMIN_GUIDE.md      # Admin functionality guide
│   ├── ARCHITECTURE.md     # System architecture
│   └── COMPONENTS.md       # Component documentation
├── fixes/                   # Fix documentation and changelogs
│   ├── AUTH_FIX_GUIDE.md   # Authentication fixes
│   ├── AUTHENTICATION_FIXED.md
│   ├── GOOGLE_AUTH_FIX.md
│   ├── PRODUCTION_AUTH_COMPARISON.md
│   ├── QUICK_FIX_AUTH.md
│   ├── SCRIPT_CLEANUP_SUMMARY.md
│   ├── SCRIPT_CONSOLIDATION_SUMMARY.md
│   ├── STOP_DEV_SAFETY_GUIDE.md
│   └── SWOT_FIX_SUMMARY.md
├── setup/                   # Setup and installation guides
│   ├── GOOGLE_AUTH_SETUP.md
│   └── GOOGLE_OAUTH_SETUP_STEPS.md
└── troubleshooting/         # Troubleshooting guides
    ├── BACKEND_PYTHON_FIX.md
    ├── MARKET_ANALYSIS_TROUBLESHOOTING.md
    ├── STARTUP_TROUBLESHOOTING.md
    └── WSL_PATH_FIX.md
```

## Quick Navigation

### Getting Started
- See the main [README.md](../README.md) in the root directory
- Development setup: [README_DEV.md](README_DEV.md)
- Quick setup: [scripts/setup/quick_setup.py](../scripts/setup/quick_setup.py)

### Architecture & Design
- [System Architecture](architecture/ARCHITECTURE.md)
- [Component Guide](architecture/COMPONENTS.md)
- [Admin Guide](architecture/ADMIN_GUIDE.md)

### Troubleshooting
- [Backend Python Issues](troubleshooting/BACKEND_PYTHON_FIX.md)
- [WSL Path Problems](troubleshooting/WSL_PATH_FIX.md)
- [Startup Issues](troubleshooting/STARTUP_TROUBLESHOOTING.md)
- [Market Analysis Issues](troubleshooting/MARKET_ANALYSIS_TROUBLESHOOTING.md)

### Setup Guides
- [Google OAuth Setup](setup/GOOGLE_OAUTH_SETUP_STEPS.md)
- [Google Auth Configuration](setup/GOOGLE_AUTH_SETUP.md)

### Development
- [CLAUDE.md](../CLAUDE.md) - AI assistant instructions
- [Scripts Organization](../scripts/README.md)

## Common Issues & Solutions

The most common issues and their solutions:

1. **Backend won't start**: See [Backend Python Fix](troubleshooting/BACKEND_PYTHON_FIX.md)
2. **WSL path errors**: See [WSL Path Fix](troubleshooting/WSL_PATH_FIX.md)
3. **Google OAuth issues**: See [Google OAuth Setup](setup/GOOGLE_OAUTH_SETUP_STEPS.md)
4. **Script organization**: See [Script Cleanup Summary](fixes/SCRIPT_CLEANUP_SUMMARY.md)

## Contributing

When adding new documentation:
1. Place architecture docs in `architecture/`
2. Place troubleshooting guides in `troubleshooting/`
3. Place setup guides in `setup/`
4. Place fix logs and changelogs in `fixes/`
5. Update this README with new entries