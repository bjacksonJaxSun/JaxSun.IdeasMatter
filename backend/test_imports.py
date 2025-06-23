#!/usr/bin/env python3
"""Test if all dependencies are properly installed"""

print("Testing imports...")
errors = []

# Test each import
imports = [
    ("FastAPI", "from fastapi import FastAPI"),
    ("Uvicorn", "import uvicorn"),
    ("SQLAlchemy", "from sqlalchemy import create_engine"),
    ("Pydantic", "from pydantic import BaseModel"),
    ("JWT/Security", "from jose import jwt"),
    ("Passlib", "from passlib.hash import bcrypt"),
    ("OpenAI", "import openai"),
    ("Anthropic", "import anthropic"),
    ("Google AI", "import google.generativeai"),
    ("LangChain", "from langchain.chat_models import ChatOpenAI"),
    ("HTTPx", "import httpx"),
    ("Redis", "import redis"),
]

for name, import_str in imports:
    try:
        exec(import_str)
        print(f"✅ {name} - OK")
    except ImportError as e:
        print(f"❌ {name} - FAILED: {e}")
        errors.append(name)

print("\n" + "="*50)
if errors:
    print(f"❌ Failed imports: {', '.join(errors)}")
    print("\nTo fix:")
    if "Pydantic" in errors:
        print("- For Pydantic: pip install pydantic==1.10.13")
    if "JWT/Security" in errors:
        print("- For JWT: pip install python-jose[cryptography]")
    if "OpenAI" in errors:
        print("- For OpenAI: pip install openai")
else:
    print("✅ All dependencies installed successfully!")
    print("\nYou can now run the full application:")
    print("  python main.py")