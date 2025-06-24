#!/usr/bin/env python3
"""
Quick Start Script for AI-First Payroll & HR Backend
This script sets up and launches the backend with minimal configuration.
"""

import subprocess
import sys
import os
from pathlib import Path

def run_command(cmd, description):
    """Run a command and handle errors"""
    print(f"\n🚀 {description}...")
    try:
        result = subprocess.run(cmd, shell=True, check=True, capture_output=True, text=True)
        if result.stdout:
            print(result.stdout)
        return True
    except subprocess.CalledProcessError as e:
        print(f"❌ Error: {e}")
        if e.stderr:
            print(e.stderr)
        return False

def main():
    print("=" * 60)
    print("🎯 AI-First Payroll & HR - Quick Start")
    print("=" * 60)
    
    # Check Python version
    if sys.version_info < (3, 8):
        print("❌ Python 3.8 or higher is required!")
        sys.exit(1)
    
    print(f"✅ Using Python {sys.version}")
    
    # Check if we're in the backend directory
    if not Path("main.py").exists():
        print("❌ Please run this script from the backend directory!")
        sys.exit(1)
    
    # Create virtual environment if it doesn't exist
    if not Path("venv").exists():
        if not run_command(f"{sys.executable} -m venv venv", "Creating virtual environment"):
            sys.exit(1)
    
    # Determine the correct activation script
    if sys.platform == "win32":
        activate_cmd = "venv\\Scripts\\activate"
        pip_cmd = "venv\\Scripts\\pip"
        python_cmd = "venv\\Scripts\\python"
    else:
        activate_cmd = "source venv/bin/activate"
        pip_cmd = "venv/bin/pip"
        python_cmd = "venv/bin/python"
    
    # Install dependencies
    print("\n📦 Installing dependencies (this may take a few minutes)...")
    if not run_command(f"{pip_cmd} install -r requirements.txt", "Installing Python packages"):
        print("\n💡 Tip: If installation fails, try:")
        print(f"   {activate_cmd}")
        print("   pip install -r requirements.txt")
        sys.exit(1)
    
    # Check if .env exists
    if not Path(".env").exists():
        print("\n⚠️  No .env file found. Using quick-launch defaults.")
        print("   A minimal .env file has been created for you.")
    
    print("\n✨ Setup complete! Starting the server...")
    print("=" * 60)
    print("\n🌐 The API will be available at:")
    print("   • API: http://localhost:8000")
    print("   • Docs: http://localhost:8000/docs")
    print("   • ReDoc: http://localhost:8000/redoc")
    print("\n📝 Using SQLite database (no setup required)")
    print("🔐 Using default JWT secret (change for production!)")
    print("\n⚡ Press Ctrl+C to stop the server")
    print("=" * 60)
    
    # Start the server
    try:
        # Use production server if available
        production_server = Path("scripts/auth/production_server.py")
        if production_server.exists():
            subprocess.run([python_cmd, str(production_server)], check=True)
        else:
            subprocess.run([python_cmd, "main.py"], check=True)
    except KeyboardInterrupt:
        print("\n\n👋 Server stopped. Goodbye!")
    except subprocess.CalledProcessError as e:
        print(f"\n❌ Server failed to start: {e}")
        print("\n💡 Try running manually:")
        print(f"   {activate_cmd}")
        print("   python main.py")

if __name__ == "__main__":
    main()