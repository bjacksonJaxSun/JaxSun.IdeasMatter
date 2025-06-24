#!/usr/bin/env python3
"""
Test SWOT API endpoint directly
"""
import sys
import os
import requests
import json
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

def test_swot_api():
    print("Testing SWOT API endpoint...")
    
    try:
        # First, let's check if we have any options in the database
        from app.core.database import SyncSessionLocal
        from app.models.research import ResearchOption
        
        with SyncSessionLocal() as db:
            options = db.query(ResearchOption).limit(1).all()
            
            if not options:
                print("No options found - test_swot_api already created one")
                return
                
            option = options[0]
            option_id = option.id
            
            print(f"Testing SWOT API for option {option_id}: {option.title}")
            
            # Test the SWOT API endpoint directly
            url = f"http://localhost:8000/api/v1/research/options/{option_id}/swot"
            payload = {
                "option_id": option_id,
                "regenerate": True
            }
            
            print(f"Making POST request to: {url}")
            
            try:
                response = requests.post(url, json=payload, timeout=10)
                print(f"Response status: {response.status_code}")
                
                if response.status_code == 200:
                    data = response.json()
                    print("SUCCESS: SWOT API endpoint working!")
                    print(f"Response keys: {list(data.keys())}")
                    
                    if 'swot' in data:
                        swot = data['swot']
                        print(f"Strengths: {len(swot.get('strengths', []))}")
                        print(f"Weaknesses: {len(swot.get('weaknesses', []))}")
                        print(f"Opportunities: {len(swot.get('opportunities', []))}")
                        print(f"Threats: {len(swot.get('threats', []))}")
                else:
                    print(f"API Error: {response.status_code}")
                    print(f"Response: {response.text}")
                    
            except requests.exceptions.ConnectionError:
                print("Connection error - backend server is not running")
                print("Please start the backend server:")
                print("1. Open terminal in backend directory")
                print("2. Run: venv/Scripts/python.exe main.py")
                
            except Exception as e:
                print(f"Request failed: {e}")
                
    except Exception as e:
        print(f"Test setup failed: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    # Change to backend directory
    if not os.path.exists("ideas_matter.db"):
        backend_dir = os.path.dirname(os.path.abspath(__file__))
        os.chdir(backend_dir)
    
    test_swot_api()