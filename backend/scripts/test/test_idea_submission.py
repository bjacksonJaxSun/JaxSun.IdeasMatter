#!/usr/bin/env python3
"""
Test script for idea submission and deletion
"""
import asyncio
import sys
import os
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from app.core.database import AsyncSessionLocal
from app.models.research import ResearchSession, ResearchOption
from app.services.research_service import ResearchService
from sqlalchemy import select

async def test_idea_submission():
    """Test idea submission process"""
    print("=== Testing Idea Submission ===")
    
    async with AsyncSessionLocal() as db:
        research_service = ResearchService(db)
        
        # Test data
        idea_title = "Test Smart Home App"
        idea_description = "A mobile app that controls smart home devices with AI automation"
        
        print(f"Submitting idea: {idea_title}")
        
        try:
            # Create a research session (simulating idea submission)
            session = ResearchSession(
                user_id=1,
                idea_id="test_123",
                title=idea_title,
                description=idea_description,
                status="researching"
            )
            
            db.add(session)
            await db.commit()
            await db.refresh(session)
            
            print(f"✅ Session created with ID: {session.id}")
            
            # Generate competitive analysis (this creates options)
            result = await research_service.generate_competitive_analysis(
                session_id=session.id,
                idea_title=idea_title,
                idea_description=idea_description
            )
            
            print(f"✅ Analysis result: {result}")
            
            # Check if options were created
            options_result = await db.execute(
                select(ResearchOption).where(ResearchOption.session_id == session.id)
            )
            options = options_result.scalars().all()
            
            print(f"✅ Created {len(options)} options:")
            for option in options:
                print(f"  - {option.title} (feasibility: {option.feasibility_score})")
            
            return session.id, len(options)
            
        except Exception as e:
            print(f"❌ Error during idea submission: {e}")
            import traceback
            traceback.print_exc()
            return None, 0

async def test_idea_deletion(session_id: int):
    """Test idea/session deletion"""
    print(f"\n=== Testing Session Deletion (ID: {session_id}) ===")
    
    async with AsyncSessionLocal() as db:
        try:
            # Check data before deletion
            session_result = await db.execute(
                select(ResearchSession).where(ResearchSession.id == session_id)
            )
            session = session_result.scalar_one_or_none()
            
            if not session:
                print(f"❌ Session {session_id} not found")
                return False
            
            options_result = await db.execute(
                select(ResearchOption).where(ResearchOption.session_id == session_id)
            )
            options = options_result.scalars().all()
            
            print(f"Before deletion: Session exists with {len(options)} options")
            
            # Delete the session (should cascade to options)
            await db.delete(session)
            await db.commit()
            
            # Verify deletion
            session_result = await db.execute(
                select(ResearchSession).where(ResearchSession.id == session_id)
            )
            session_after = session_result.scalar_one_or_none()
            
            options_result = await db.execute(
                select(ResearchOption).where(ResearchOption.session_id == session_id)
            )
            options_after = options_result.scalars().all()
            
            if session_after is None and len(options_after) == 0:
                print("✅ Session and related options deleted successfully")
                return True
            else:
                print(f"❌ Deletion failed: Session={session_after}, Options={len(options_after)}")
                return False
                
        except Exception as e:
            print(f"❌ Error during deletion: {e}")
            import traceback
            traceback.print_exc()
            return False

async def check_database_state():
    """Check current database state"""
    print("\n=== Current Database State ===")
    
    async with AsyncSessionLocal() as db:
        try:
            # Count sessions
            sessions_result = await db.execute(select(ResearchSession))
            sessions = sessions_result.scalars().all()
            
            # Count options
            options_result = await db.execute(select(ResearchOption))
            options = options_result.scalars().all()
            
            print(f"Total sessions: {len(sessions)}")
            print(f"Total options: {len(options)}")
            
            if sessions:
                print("\nSessions:")
                for session in sessions:
                    session_options = await db.execute(
                        select(ResearchOption).where(ResearchOption.session_id == session.id)
                    )
                    option_count = len(session_options.scalars().all())
                    print(f"  - ID {session.id}: {session.title} ({option_count} options)")
            
        except Exception as e:
            print(f"❌ Error checking database: {e}")

async def main():
    """Main test function"""
    print("=== Idea Submission and Deletion Test ===\n")
    
    try:
        # Check initial state
        await check_database_state()
        
        # Test idea submission
        session_id, option_count = await test_idea_submission()
        
        if session_id and option_count > 0:
            print(f"\n✅ Idea submission successful!")
            
            # Test deletion
            deletion_success = await test_idea_deletion(session_id)
            
            if deletion_success:
                print(f"\n✅ Idea deletion successful!")
            else:
                print(f"\n❌ Idea deletion failed!")
        else:
            print(f"\n❌ Idea submission failed!")
        
        # Check final state
        await check_database_state()
        
        print("\n=== Test Complete ===")
        
    except Exception as e:
        print(f"❌ Test failed: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    # Change to backend directory
    if not os.path.exists("ideas_matter.db"):
        backend_dir = os.path.dirname(os.path.abspath(__file__))
        os.chdir(backend_dir)
    
    asyncio.run(main())