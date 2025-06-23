#!/usr/bin/env python3
"""Create a test user for debugging"""
import asyncio
import sys
import os

# Add the current directory to Python path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from sqlalchemy.ext.asyncio import create_async_engine, AsyncSession
from sqlalchemy.orm import sessionmaker
from app.core.config import get_settings
from app.core.security import get_password_hash
from app.models.user import User
from app.core.database import Base

settings = get_settings()

async def create_test_user():
    """Create a test user in the database"""
    # Create engine
    engine = create_async_engine(settings.DATABASE_URL, echo=True)
    
    # Create tables
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
    
    # Create session
    AsyncSessionLocal = sessionmaker(
        engine, class_=AsyncSession, expire_on_commit=False
    )
    
    async with AsyncSessionLocal() as db:
        # Check if test user exists
        from sqlalchemy import select
        result = await db.execute(select(User).where(User.email == "test@example.com"))
        existing_user = result.scalar_one_or_none()
        
        if existing_user:
            print("❌ Test user already exists!")
            print(f"Email: {existing_user.email}")
            print(f"Name: {existing_user.name}")
        else:
            # Create test user
            test_user = User(
                email="test@example.com",
                hashed_password=get_password_hash("password123"),
                name="Test User",
                role="user",
                is_active=True,
                is_verified=True
            )
            db.add(test_user)
            await db.commit()
            print("✅ Test user created successfully!")
            print("Email: test@example.com")
            print("Password: password123")
        
        # Also create an admin user
        result = await db.execute(select(User).where(User.email == "admin@example.com"))
        admin_exists = result.scalar_one_or_none()
        
        if not admin_exists:
            admin_user = User(
                email="admin@example.com",
                hashed_password=get_password_hash("admin123"),
                name="Admin User",
                role="admin",
                is_active=True,
                is_verified=True
            )
            db.add(admin_user)
            await db.commit()
            print("\n✅ Admin user created successfully!")
            print("Email: admin@example.com")
            print("Password: admin123")

if __name__ == "__main__":
    print("Creating test users...")
    asyncio.run(create_test_user())