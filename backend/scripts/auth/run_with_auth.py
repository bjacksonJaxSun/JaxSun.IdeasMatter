#!/usr/bin/env python3
"""
Wrapper to run the enhanced auth server when main.py is called
This allows start_dev.bat to work without modification
"""

import sys
import os

# Add the current directory to Python path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

# Import and run the auth server
from auth_server_with_google import start_auth_server

if __name__ == "__main__":
    print("=" * 60)
    print("Running Enhanced Auth Server (main.py wrapper)")
    print("=" * 60)
    start_auth_server()