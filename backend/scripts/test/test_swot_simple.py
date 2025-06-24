#!/usr/bin/env python3
"""
Simple test for SWOT analysis functionality
"""
import sys
import os
import asyncio
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

# Test if we can import the required modules
def test_imports():
    print("Testing imports...")
    try:
        from app.services.swot_service import SwotAnalysisService
        print("✅ SwotAnalysisService imported successfully")
    except Exception as e:
        print(f"❌ Failed to import SwotAnalysisService: {e}")
        return False
    
    try:
        from app.services.ai_orchestration_simple import ai_orchestrator
        print("✅ AI orchestrator imported successfully")
    except Exception as e:
        print(f"❌ Failed to import AI orchestrator: {e}")
        return False
    
    try:
        from app.core.database import SyncSessionLocal
        print("✅ Sync database session imported successfully")
    except Exception as e:
        print(f"❌ Failed to import sync database: {e}")
        return False
    
    return True

def test_swot_service():
    print("\nTesting SWOT service initialization...")
    try:
        from app.services.swot_service import SwotAnalysisService
        from app.services.ai_orchestration_simple import ai_orchestrator
        
        swot_service = SwotAnalysisService(ai_orchestrator)
        print("✅ SWOT service initialized successfully")
        return swot_service
    except Exception as e:
        print(f"❌ Failed to initialize SWOT service: {e}")
        import traceback
        traceback.print_exc()
        return None

def test_database_connection():
    print("\nTesting database connection...")
    try:
        from app.core.database import SyncSessionLocal
        from app.models.research import ResearchOption, ResearchSession
        
        with SyncSessionLocal() as db:
            # Try to query options
            options = db.query(ResearchOption).limit(5).all()
            sessions = db.query(ResearchSession).limit(5).all()
            
            print(f"✅ Database connection successful")
            print(f"  - Found {len(sessions)} sessions")
            print(f"  - Found {len(options)} options")
            
            if options:
                print(f"  - Sample option: {options[0].title}")
                return options[0].id
            else:
                print("  - No options available for testing")
                return None
                
    except Exception as e:
        print(f"❌ Database connection failed: {e}")
        import traceback
        traceback.print_exc()
        return None

def test_ai_service():
    print("\nTesting AI service...")
    try:
        from app.services.ai_orchestration_simple import ai_orchestrator
        
        # Try a simple AI call
        test_prompt = "Generate a simple SWOT analysis for a mobile app business idea."
        response = ai_orchestrator.process_message(test_prompt, "test")
        
        print(f"✅ AI service working")
        print(f"  - Response length: {len(response)} characters")
        print(f"  - Sample response: {response[:100]}...")
        return True
        
    except Exception as e:
        print(f"❌ AI service failed: {e}")
        import traceback
        traceback.print_exc()
        return False

def create_test_option():
    print("\nCreating test option for SWOT analysis...")
    try:
        from app.core.database import SyncSessionLocal
        from app.models.research import ResearchOption, ResearchSession
        
        with SyncSessionLocal() as db:
            # Create a test session first
            session = ResearchSession(
                user_id=1,
                idea_id="test_swot",
                title="Test SWOT Idea",
                description="A test idea for SWOT analysis",
                status="active"
            )
            db.add(session)
            db.commit()
            db.refresh(session)
            
            # Create a test option
            option = ResearchOption(
                session_id=session.id,
                category="business_model",
                title="Direct Market Entry",
                description="Launch the product directly to the target market",
                pros=["Quick market entry", "High impact"],
                cons=["Higher risk", "More investment needed"],
                feasibility_score=0.7,
                impact_score=0.8,
                risk_score=0.6,
                recommended=True
            )
            db.add(option)
            db.commit()
            db.refresh(option)
            
            print(f"✅ Test option created with ID: {option.id}")
            return option.id
            
    except Exception as e:
        print(f"❌ Failed to create test option: {e}")
        import traceback
        traceback.print_exc()
        return None

def test_swot_generation(option_id: int):
    print(f"\nTesting SWOT generation for option {option_id}...")
    try:
        from app.services.swot_service import SwotAnalysisService
        from app.services.ai_orchestration_simple import ai_orchestrator
        from app.core.database import SyncSessionLocal
        
        swot_service = SwotAnalysisService(ai_orchestrator)
        
        with SyncSessionLocal() as db:
            swot = swot_service.generate_swot_analysis(
                db=db,
                option_id=option_id,
                regenerate=True
            )
            
            print("✅ SWOT analysis generated successfully!")
            print(f"  - Strengths: {len(swot.strengths)} items")
            print(f"  - Weaknesses: {len(swot.weaknesses)} items")
            print(f"  - Opportunities: {len(swot.opportunities)} items")
            print(f"  - Threats: {len(swot.threats)} items")
            print(f"  - Confidence: {swot.confidence}")
            
            return True
            
    except Exception as e:
        print(f"❌ SWOT generation failed: {e}")
        import traceback
        traceback.print_exc()
        return False

def main():
    print("=== SWOT Analysis Diagnostic Test ===\n")
    
    # Change to backend directory
    if not os.path.exists("ideas_matter.db"):
        backend_dir = os.path.dirname(os.path.abspath(__file__))
        os.chdir(backend_dir)
    
    # Test sequence
    if not test_imports():
        print("\n❌ Import test failed - stopping")
        return
    
    if not test_swot_service():
        print("\n❌ SWOT service test failed - stopping")
        return
    
    option_id = test_database_connection()
    if option_id is None:
        print("\nNo existing options found, creating test option...")
        option_id = create_test_option()
    
    if option_id is None:
        print("\n❌ Could not get or create test option - stopping")
        return
    
    # Test AI service
    if not test_ai_service():
        print("\n⚠️ AI service test failed - SWOT will use fallback")
    
    # Test SWOT generation
    if test_swot_generation(option_id):
        print("\n✅ All tests passed! SWOT analysis should work.")
    else:
        print("\n❌ SWOT generation test failed!")
    
    print("\n=== Test Complete ===")

if __name__ == "__main__":
    main()