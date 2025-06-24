#!/usr/bin/env python3
"""
Test script to verify the research sessions endpoint fix
"""
import asyncio
import sys
from pathlib import Path

# Add the backend directory to the path
sys.path.insert(0, str(Path(__file__).parent))

from app.api.v1.research import get_research_session
from app.core.database import get_db
from app.models.research import ResearchSession, ResearchConversation, ResearchInsight, ResearchOption
from sqlalchemy.ext.asyncio import AsyncSession

async def test_endpoint():
    """Test the research session endpoint directly"""
    try:
        # Get a database session
        async for db in get_db():
            # Test with a session ID that exists (assuming session 4 exists based on logs)
            try:
                result = await get_research_session(4, db)
                print(f"✅ Endpoint test successful!")
                print(f"Session ID: {result.id}")
                print(f"Title: {result.title}")
                print(f"Status: {result.status}")
                print(f"Conversations: {len(result.conversations)}")
                print(f"Insights: {len(result.insights)}")
                print(f"Options: {len(result.options)}")
                return True
            except Exception as e:
                print(f"❌ Endpoint test failed: {e}")
                import traceback
                traceback.print_exc()
                return False
    except Exception as e:
        print(f"❌ Database connection failed: {e}")
        return False

if __name__ == "__main__":
    result = asyncio.run(test_endpoint())
    sys.exit(0 if result else 1)