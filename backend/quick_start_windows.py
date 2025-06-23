#!/usr/bin/env python3
"""
Quick start script for Windows - bypasses complex dependencies
"""
import os
import uvicorn
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
import sqlite3
from pathlib import Path

# Simple FastAPI app without complex dependencies
app = FastAPI(
    title="Ideas Matter - Quick Start",
    description="Simple version for Windows development",
    version="1.0.0"
)

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:4000", "http://localhost:3000"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Simple in-memory storage for development
users_db = {}
sessions_db = {}

@app.get("/")
async def root():
    return {
        "message": "Ideas Matter API - Windows Quick Start",
        "version": "1.0.0",
        "status": "running"
    }

@app.get("/health")
async def health():
    return {"status": "healthy"}

# Simple auth endpoints
@app.post("/api/v1/auth/login")
async def login(credentials: dict):
    email = credentials.get("username", "")
    password = credentials.get("password", "")
    
    # Simple demo authentication
    role = "user"
    if "admin" in email.lower():
        role = "system_admin"
    
    return {
        "access_token": f"demo-token-{email}",
        "refresh_token": f"refresh-{email}",
        "token_type": "bearer",
        "expires_in": 3600
    }

@app.post("/api/v1/auth/register")
async def register(user_data: dict):
    email = user_data.get("email")
    name = user_data.get("name")
    
    return {
        "access_token": f"demo-token-{email}",
        "refresh_token": f"refresh-{email}",
        "token_type": "bearer",
        "expires_in": 3600
    }

@app.post("/api/v1/auth/google")
async def google_auth(auth_data: dict):
    return {
        "access_token": "demo-google-token",
        "refresh_token": "demo-google-refresh",
        "token_type": "bearer",
        "expires_in": 3600
    }

@app.get("/api/v1/auth/me")
async def get_current_user():
    return {
        "id": 1,
        "email": "demo@example.com",
        "name": "Demo User",
        "role": "system_admin",
        "tenant_id": None,
        "permissions": ["*"],
        "picture": None
    }

@app.post("/api/v1/auth/logout")
async def logout():
    return {"message": "Logged out successfully"}

# Simple research endpoints
@app.post("/api/v1/research/sessions")
async def create_research_session(session_data: dict):
    session_id = len(sessions_db) + 1
    session = {
        "id": session_id,
        "title": session_data.get("title", "New Research Session"),
        "description": session_data.get("description", ""),
        "user_id": 1,
        "status": "active",
        "created_at": "2024-01-01T00:00:00Z"
    }
    sessions_db[session_id] = session
    return session

@app.get("/api/v1/research/sessions/{session_id}")
async def get_research_session(session_id: int):
    if session_id not in sessions_db:
        raise HTTPException(status_code=404, detail="Session not found")
    
    session = sessions_db[session_id].copy()
    session.update({
        "conversations": [],
        "insights": [
            {
                "id": 1,
                "category": "target_market",
                "title": "Tech professionals",
                "description": "Target market of tech-savvy professionals",
                "confidence_score": 0.8,
                "is_validated": False
            }
        ],
        "options": [
            {
                "id": 1,
                "category": "business_model",
                "title": "SaaS Subscription",
                "description": "Monthly subscription model",
                "pros": ["Predictable revenue", "Scalable"],
                "cons": ["Customer acquisition cost"],
                "feasibility_score": 0.9,
                "impact_score": 0.8,
                "risk_score": 0.3,
                "recommended": True
            }
        ]
    })
    return session

@app.post("/api/v1/research/sessions/{session_id}/brainstorm")
async def brainstorm(session_id: int, request: dict):
    message = request.get("message", "")
    
    # Simple AI response simulation
    response = {
        "message": f"Great question about '{message}'. Let me help you explore this idea further. Based on market research, I can see several opportunities.",
        "insights": [
            {
                "category": "target_market",
                "title": "Primary audience identified",
                "description": f"Based on your question about '{message}', the target market appears to be professionals seeking efficiency.",
                "confidence_score": 0.7
            }
        ],
        "options": [
            {
                "category": "business_model",
                "title": "Freemium approach",
                "description": "Start with free tier to build user base",
                "pros": ["Low barrier to entry", "User growth"],
                "cons": ["Conversion challenges"],
                "feasibility_score": 0.8,
                "impact_score": 0.6,
                "risk_score": 0.4
            }
        ],
        "follow_up_questions": [
            "What specific pain points are you trying to solve?",
            "Have you validated this with potential customers?",
            "What's your timeline for launch?"
        ]
    }
    
    return response

if __name__ == "__main__":
    print("🚀 Starting Ideas Matter - Windows Quick Start")
    print("📍 Backend will be available at: http://localhost:8000")
    print("📖 API docs available at: http://localhost:8000/docs")
    print("💡 This is a simplified version for Windows development")
    
    uvicorn.run(
        "quick_start_windows:app",
        host="0.0.0.0",
        port=8000,
        reload=True
    )