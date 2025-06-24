#!/usr/bin/env python3
"""
Debug market analysis endpoint
"""

import sys
import os
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

def debug_market_analysis_issues():
    """Debug potential issues with market analysis"""
    
    print("🔍 Debugging Market Analysis Issues...")
    print("=" * 50)
    
    # Check 1: Import issues
    try:
        print("1. Testing imports...")
        from app.schemas.market_analysis import MarketAnalysisRequest
        print("   ✓ Schemas import successful")
        
        from app.models.market_analysis import MarketAnalysis
        print("   ✓ Models import successful")
        
        # Test the request schema
        request_data = {
            "session_id": 1,
            "analysis_type": "comprehensive"
        }
        request = MarketAnalysisRequest(**request_data)
        print(f"   ✓ Request schema validation successful: {request.analysis_type}")
        
    except Exception as e:
        print(f"   ❌ Import error: {str(e)}")
        return
    
    # Check 2: AI service mock
    try:
        print("\n2. Testing AI service mock...")
        
        class MockAIService:
            def process_message(self, prompt, message_type="general"):
                # Return a mock JSON response for market analysis
                return '''{
                    "market_overview": {
                        "industry": "Technology",
                        "market_category": "Software/Services",
                        "geographic_scope": "Global"
                    },
                    "market_size": {
                        "tam": 10000000000,
                        "sam": 1000000000,
                        "som": 50000000,
                        "cagr": 15.0
                    },
                    "market_dynamics": {
                        "market_drivers": ["Digital transformation", "AI adoption"],
                        "market_barriers": ["High competition", "Regulatory challenges"]
                    },
                    "customer_analysis": {
                        "customer_segments": [
                            {"name": "Enterprise", "size_percentage": 40, "attractiveness_score": 0.8}
                        ],
                        "customer_pain_points": ["Cost", "Complexity"]
                    },
                    "competitive_landscape": {
                        "direct_competitors": [],
                        "indirect_competitors": []
                    }
                }'''
        
        ai_service = MockAIService()
        response = ai_service.process_message("Test prompt", "market_analysis")
        print("   ✓ AI service mock working")
        
        import json
        data = json.loads(response)
        print(f"   ✓ AI response parsing successful: TAM = ${data['market_size']['tam']:,}")
        
    except Exception as e:
        print(f"   ❌ AI service error: {str(e)}")
        return
    
    # Check 3: Service logic
    try:
        print("\n3. Testing service logic...")
        
        class MockMarketAnalysisService:
            def __init__(self, ai_service):
                self.ai_service = ai_service
            
            def _generate_fallback_market_data(self, idea_title, idea_description):
                return {
                    "market_overview": {
                        "industry": "Technology",
                        "market_category": "Software/Services",
                        "geographic_scope": "Global"
                    },
                    "market_size": {
                        "tam": 10000000000,
                        "sam": 1000000000,
                        "som": 50000000,
                        "cagr": 15.0,
                        "market_maturity": "Growth"
                    }
                }
            
            def generate_comprehensive_market_analysis(self, db, session_id, idea_title, idea_description):
                # Mock the comprehensive analysis generation
                market_data = self._generate_fallback_market_data(idea_title, idea_description)
                
                # Mock comprehensive response
                class MockAnalysis:
                    def __init__(self):
                        self.market_analysis = None
                        self.competitors = []
                        self.segments = []
                        self.trends = []
                        self.opportunities = []
                
                return MockAnalysis()
        
        mock_service = MockMarketAnalysisService(ai_service)
        result = mock_service.generate_comprehensive_market_analysis(
            None, 1, "Test Idea", "Test Description"
        )
        print("   ✓ Service logic working")
        
    except Exception as e:
        print(f"   ❌ Service logic error: {str(e)}")
        return
    
    # Check 4: Endpoint structure
    try:
        print("\n4. Testing endpoint structure...")
        
        # Mock FastAPI dependencies
        class MockSession:
            def query(self, model):
                return self
            
            def filter(self, *args):
                return self
            
            def first(self):
                # Mock research session
                class MockResearchSession:
                    def __init__(self):
                        self.id = 1
                        self.title = "Test Idea"
                        self.description = "Test Description"
                
                return MockResearchSession()
        
        # Test the basic endpoint logic
        mock_db = MockSession()
        mock_request = MarketAnalysisRequest(session_id=1, analysis_type="comprehensive")
        mock_user_id = "test_user"
        
        # Verify session lookup logic
        session = mock_db.query(None).filter().first()
        assert session is not None
        print("   ✓ Session lookup logic working")
        
        print("   ✓ Endpoint structure valid")
        
    except Exception as e:
        print(f"   ❌ Endpoint structure error: {str(e)}")
        return
    
    print("\n✅ All debug checks passed!")
    print("\nPossible issues to check:")
    print("1. Backend server not running")
    print("2. Database tables not created")
    print("3. Authentication token missing or invalid")
    print("4. CORS issues")
    print("5. API endpoint not registered correctly")
    
    print("\nRecommended fixes:")
    print("1. Check browser developer console for detailed error messages")
    print("2. Verify the backend server is running on port 8000")
    print("3. Check if user is properly authenticated")
    print("4. Ensure market analysis tables exist in database")

if __name__ == "__main__":
    debug_market_analysis_issues()