#!/usr/bin/env python3
"""
Quick setup script for Ideas Matter
Sets up the development environment with minimal configuration
"""

import os
import sys
import subprocess
import shutil
from pathlib import Path

def print_header(text):
    """Print a formatted header"""
    print("\n" + "=" * 50)
    print(f"  {text}")
    print("=" * 50 + "\n")

def check_command(command):
    """Check if a command is available"""
    try:
        subprocess.run([command, "--version"], capture_output=True, check=True)
        return True
    except (subprocess.CalledProcessError, FileNotFoundError):
        return False

def setup_backend():
    """Setup backend environment"""
    print_header("Setting up Backend")
    
    backend_path = Path(__file__).parent.parent.parent / "backend"
    os.chdir(backend_path)
    
    # Create virtual environment
    if not (backend_path / "venv").exists():
        print("Creating virtual environment...")
        subprocess.run([sys.executable, "-m", "venv", "venv"])
    
    # Determine pip path
    if sys.platform == "win32":
        pip_path = backend_path / "venv" / "Scripts" / "pip.exe"
        activate_cmd = str(backend_path / "venv" / "Scripts" / "activate.bat")
    else:
        pip_path = backend_path / "venv" / "bin" / "pip"
        activate_cmd = f"source {backend_path / 'venv' / 'bin' / 'activate'}"
    
    # Install requirements
    print("Installing Python dependencies...")
    subprocess.run([str(pip_path), "install", "--upgrade", "pip"])
    subprocess.run([str(pip_path), "install", "-r", "requirements.txt"])
    
    # Create .env file if it doesn't exist
    env_file = backend_path / ".env"
    env_example = backend_path / ".env.example"
    
    if not env_file.exists() and env_example.exists():
        print("Creating .env file from template...")
        shutil.copy(env_example, env_file)
        
        # Set default values for quick start
        with open(env_file, "a") as f:
            f.write("\n# Quick Setup Defaults\n")
            f.write("QUICK_LAUNCH=True\n")
            f.write("DATABASE_TYPE=sqlite\n")
            f.write("JWT_SECRET_KEY=dev-secret-key-change-in-production\n")
            f.write("ADMIN_EMAILS=admin@example.com\n")
    
    print(f"✅ Backend setup complete!")
    print(f"   To activate: {activate_cmd}")

def setup_frontend():
    """Setup frontend environment"""
    print_header("Setting up Frontend")
    
    frontend_path = Path(__file__).parent.parent.parent / "frontend"
    os.chdir(frontend_path)
    
    # Install dependencies
    if not (frontend_path / "node_modules").exists():
        print("Installing Node.js dependencies...")
        subprocess.run(["npm", "install"])
    else:
        print("Node modules already installed, updating...")
        subprocess.run(["npm", "update"])
    
    # Create .env file if needed
    env_file = frontend_path / ".env"
    if not env_file.exists():
        print("Creating frontend .env file...")
        with open(env_file, "w") as f:
            f.write("VITE_API_URL=http://localhost:8000\n")
            f.write("VITE_APP_NAME=Ideas Matter\n")
    
    print("✅ Frontend setup complete!")

def setup_database():
    """Initialize database"""
    print_header("Setting up Database")
    
    backend_path = Path(__file__).parent.parent.parent / "backend"
    os.chdir(backend_path)
    
    # Run alembic migrations
    print("Running database migrations...")
    if sys.platform == "win32":
        alembic_path = backend_path / "venv" / "Scripts" / "alembic.exe"
    else:
        alembic_path = backend_path / "venv" / "bin" / "alembic"
    
    if alembic_path.exists():
        subprocess.run([str(alembic_path), "upgrade", "head"])
        print("✅ Database setup complete!")
    else:
        print("⚠️  Alembic not found, skipping migrations")

def main():
    print_header("Ideas Matter Quick Setup")
    
    # Check prerequisites
    print("Checking prerequisites...")
    
    if not check_command("python"):
        print("❌ Python is not installed or not in PATH")
        print("   Please install Python 3.8+ from https://python.org")
        return 1
    
    if not check_command("node"):
        print("❌ Node.js is not installed or not in PATH")
        print("   Please install Node.js 16+ from https://nodejs.org")
        return 1
    
    print("✅ All prerequisites found!")
    
    # Run setup
    try:
        setup_backend()
        setup_frontend()
        setup_database()
        
        print_header("Setup Complete! 🎉")
        print("To start the development servers:")
        print("  Windows: scripts\\dev\\start_dev.bat")
        print("  PowerShell: scripts\\dev\\start_dev.ps1")
        print("\nServers will run on:")
        print("  Backend: http://localhost:8000")
        print("  Frontend: http://localhost:4000")
        print("  API Docs: http://localhost:8000/docs")
        
        return 0
        
    except Exception as e:
        print(f"\n❌ Setup failed: {e}")
        return 1

if __name__ == "__main__":
    sys.exit(main())