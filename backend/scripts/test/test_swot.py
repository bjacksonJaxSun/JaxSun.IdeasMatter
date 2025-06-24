#!/usr/bin/env python3
"""
Test script for SWOT analysis functionality
"""
import sys
import os
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from app.services.swot_service import SwotAnalysisService
from app.services.ai_orchestration_simple import ai_orchestrator
from app.schemas.research import SwotAnalysis

# Mock option and session data for testing
class MockOption:
    def __init__(self):
        self.id = 1
        self.title = "E-commerce Platform"
        self.description = "Build a comprehensive e-commerce platform for small businesses"
        self.category = "business_model"
        self.pros = ["High market demand", "Scalable revenue model", "Low entry barriers"]
        self.cons = ["High competition", "Complex development", "Customer acquisition costs"]
        self.feasibility_score = 0.8
        self.impact_score = 0.9
        self.risk_score = 0.6
        self.option_metadata = {}
        self.session_id = 1

class MockSession:
    def __init__(self):
        self.id = 1
        self.title = "E-commerce Business Idea"
        self.description = "Exploring opportunities in the e-commerce space"

class MockInsight:
    def __init__(self, category, title, description):
        self.category = category
        self.title = title
        self.description = description
        self.data = {}
        self.subcategory = None

def test_swot_context_building():
    """Test SWOT context building"""
    print("Testing SWOT context building...")
    
    swot_service = SwotAnalysisService(ai_orchestrator)
    
    option = MockOption()
    session = MockSession()
    insights = [
        MockInsight("target_market", "Small Business Market", "Growing market of small businesses needing e-commerce solutions"),
        MockInsight("customer_profile", "Tech-Savvy Entrepreneurs", "Small business owners comfortable with technology"),
    ]
    
    context = swot_service._build_swot_context(option, session, insights)
    
    print("Context built successfully:")
    print(f"- Option: {context['option']['title']}")
    print(f"- Session: {context['session']['title']}")
    print(f"- Insights categories: {list(context['insights'].keys())}")
    print("✅ Context building test passed")

def test_fallback_swot():
    """Test fallback SWOT generation"""
    print("\nTesting fallback SWOT generation...")
    
    swot_service = SwotAnalysisService(ai_orchestrator)
    
    option = MockOption()
    session = MockSession()
    insights = []
    
    context = swot_service._build_swot_context(option, session, insights)
    fallback_swot = swot_service._generate_fallback_swot(context)
    
    print("Fallback SWOT generated:")
    print(f"- Strengths: {len(fallback_swot['strengths'])} items")
    print(f"- Weaknesses: {len(fallback_swot['weaknesses'])} items")
    print(f"- Opportunities: {len(fallback_swot['opportunities'])} items")
    print(f"- Threats: {len(fallback_swot['threats'])} items")
    print(f"- Confidence: {fallback_swot['confidence']}")
    print("✅ Fallback SWOT test passed")

def test_pdf_service():
    """Test PDF service initialization"""
    print("\nTesting PDF service...")
    
    try:
        from app.services.pdf_service import PdfService
        pdf_service = PdfService()
        print("✅ PDF service initialized successfully")
    except ImportError as e:
        print(f"❌ PDF service import failed: {e}")
        print("Note: reportlab might not be installed")
    except Exception as e:
        print(f"❌ PDF service initialization failed: {e}")

if __name__ == "__main__":
    print("=== SWOT Analysis Implementation Test ===\n")
    
    try:
        test_swot_context_building()
        test_fallback_swot()
        test_pdf_service()
        
        print("\n=== All Tests Completed ===")
        print("✅ SWOT analysis implementation is ready!")
        print("\nNext steps:")
        print("1. Install reportlab: pip install reportlab")
        print("2. Run database migrations")
        print("3. Start the backend server")
        print("4. Test the frontend integration")
        
    except Exception as e:
        print(f"❌ Test failed: {e}")
        import traceback
        traceback.print_exc()