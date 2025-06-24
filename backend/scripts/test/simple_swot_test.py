#!/usr/bin/env python3
"""
Simple SWOT test without Unicode characters
"""
import sys
import os
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

def test_swot():
    print("Testing SWOT analysis...")
    
    try:
        # Test imports
        from app.services.swot_service import SwotAnalysisService
        from app.services.ai_orchestration_simple import ai_orchestrator
        from app.core.database import SyncSessionLocal
        from app.models.research import ResearchOption
        
        print("All imports successful")
        
        # Initialize SWOT service
        swot_service = SwotAnalysisService(ai_orchestrator)
        print("SWOT service initialized")
        
        # Test database connection
        with SyncSessionLocal() as db:
            options = db.query(ResearchOption).limit(1).all()
            print(f"Found {len(options)} options in database")
            
            if options:
                option = options[0]
                print(f"Testing with option: {option.title}")
                
                # Test SWOT generation
                swot = swot_service.generate_swot_analysis(
                    db=db,
                    option_id=option.id,
                    regenerate=True
                )
                
                print("SWOT analysis generated!")
                print(f"Strengths: {len(swot.strengths)}")
                print(f"Weaknesses: {len(swot.weaknesses)}")
                print(f"Opportunities: {len(swot.opportunities)}")
                print(f"Threats: {len(swot.threats)}")
                print(f"Confidence: {swot.confidence}")
                
                return True
            else:
                print("No options found - creating test option")
                # Create test option and session
                from app.models.research import ResearchSession
                
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
                
                option = ResearchOption(
                    session_id=session.id,
                    category="business_model",
                    title="Test Market Entry",
                    description="Test market entry strategy",
                    pros=["Quick entry", "High impact"],
                    cons=["High risk", "More investment"],
                    feasibility_score=0.7,
                    impact_score=0.8,
                    risk_score=0.6,
                    recommended=True
                )
                db.add(option)
                db.commit()
                db.refresh(option)
                
                print(f"Created test option with ID: {option.id}")
                
                # Test SWOT generation
                swot = swot_service.generate_swot_analysis(
                    db=db,
                    option_id=option.id,
                    regenerate=True
                )
                
                print("SWOT analysis generated!")
                print(f"Strengths: {len(swot.strengths)}")
                print(f"Weaknesses: {len(swot.weaknesses)}")
                print(f"Opportunities: {len(swot.opportunities)}")
                print(f"Threats: {len(swot.threats)}")
                print(f"Confidence: {swot.confidence}")
                
                return True
        
    except Exception as e:
        print(f"Error: {e}")
        import traceback
        traceback.print_exc()
        return False

if __name__ == "__main__":
    # Change to backend directory
    if not os.path.exists("ideas_matter.db"):
        backend_dir = os.path.dirname(os.path.abspath(__file__))
        os.chdir(backend_dir)
    
    if test_swot():
        print("\nSUCCESS: SWOT analysis is working!")
    else:
        print("\nFAILED: SWOT analysis has issues")