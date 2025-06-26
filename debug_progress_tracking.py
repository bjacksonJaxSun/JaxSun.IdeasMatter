#!/usr/bin/env python3
"""
Debug script to test the exact Cat and Mouse Game progress tracking scenario
that users are experiencing
"""
import requests
import json
import time
import sys
from typing import Dict, Any

# Global base URL that will be updated when backend is found
base_url = "http://localhost:8000"

def print_header(message: str):
    print(f"\n{'='*60}")
    print(f" {message}")
    print(f"{'='*60}\n")

def print_success(message: str):
    print(f"✅ SUCCESS: {message}")

def print_error(message: str):
    print(f"❌ ERROR: {message}")

def print_info(message: str):
    print(f"ℹ️  INFO: {message}")

def print_warning(message: str):
    print(f"⚠️  WARNING: {message}")

def test_backend_health():
    """Test if backend is running and healthy"""
    # Try different possible addresses
    urls = [
        "http://localhost:8000/health",
        "http://127.0.0.1:8000/health", 
        "http://0.0.0.0:8000/health"
    ]
    
    for url in urls:
        try:
            response = requests.get(url, timeout=5)
            if response.status_code == 200:
                print_success(f"Backend server is running at {url}")
                # Update base URL for other functions
                global base_url
                base_url = url.replace("/health", "")
                return True
            else:
                print_error(f"Backend health check failed at {url}: {response.status_code}")
        except Exception as e:
            print_info(f"Cannot connect to {url}: {e}")
    
    print_error("Backend is not accessible at any tested address")
    return False

def get_auth_token():
    """Get authentication token via bypass"""
    try:
        response = requests.post(
            f"{base_url}/api/v1/auth/bypass",
            json={"role": "user"},
            timeout=10
        )
        
        if response.status_code == 200:
            token = response.json().get("access_token")
            if token:
                print_success("Authentication successful via bypass")
                return token
            else:
                print_error("No access token in response")
                return None
        else:
            print_error(f"Authentication failed: {response.status_code} - {response.text}")
            return None
            
    except Exception as e:
        print_error(f"Authentication error: {e}")
        return None

def test_research_endpoints_direct(token: str):
    """Test research endpoints directly"""
    headers = {"Authorization": f"Bearer {token}"} if token else {}
    
    print_info("Testing research endpoints directly...")
    
    # Test 1: Get approaches
    try:
        response = requests.get(
            f"{base_url}/api/v1/research-strategy/approaches",
            headers=headers,
            timeout=10
        )
        
        if response.status_code == 200:
            approaches = response.json()
            print_success(f"Approaches endpoint working - {len(approaches)} approaches available")
        else:
            print_error(f"Approaches endpoint failed: {response.status_code} - {response.text}")
            return False
    except Exception as e:
        print_error(f"Approaches endpoint error: {e}")
        return False
    
    # Test 2: Initiate Cat and Mouse Game strategy
    strategy_data = {
        "session_id": 1,
        "approach": "market_deep_dive",  # This is what user selected
        "custom_parameters": {
            "idea_title": "Cat and Mouse Game",
            "idea_description": "This is a game that 4 - 10 year olds can play and the user choses their favorite cat and mouse. The game allows the user to learn how strategy works by being either the cat or mouse."
        }
    }
    
    try:
        response = requests.post(
            f"{base_url}/api/v1/research-strategy/initiate",
            json=strategy_data,
            headers=headers,
            timeout=15
        )
        
        if response.status_code == 200:
            result = response.json()
            strategy_id = result.get("strategy", {}).get("id")
            print_success(f"Strategy initiation successful - ID: {strategy_id}")
            return strategy_id
        else:
            print_error(f"Strategy initiation failed: {response.status_code} - {response.text}")
            return None
            
    except Exception as e:
        print_error(f"Strategy initiation error: {e}")
        return None

def test_execution_and_progress(strategy_id: int, token: str):
    """Test strategy execution and progress tracking"""
    headers = {"Authorization": f"Bearer {token}"} if token else {}
    
    # Execute strategy
    try:
        response = requests.post(
            f"{base_url}/api/v1/research-strategy/execute/{strategy_id}",
            headers=headers,
            timeout=15
        )
        
        if response.status_code == 200:
            print_success("Strategy execution started successfully")
        else:
            print_error(f"Strategy execution failed: {response.status_code} - {response.text}")
            return False
            
    except Exception as e:
        print_error(f"Strategy execution error: {e}")
        return False
    
    # Test progress tracking multiple times
    print_info("Testing progress tracking (this is where the error occurs)...")
    
    for attempt in range(5):
        time.sleep(2)  # Wait a bit between attempts
        
        try:
            response = requests.get(
                f"{base_url}/api/v1/research-strategy/progress/{strategy_id}",
                headers=headers,
                timeout=10
            )
            
            if response.status_code == 200:
                progress = response.json()
                print_success(f"Attempt {attempt + 1}: Progress tracking working - {progress.get('progress_percentage', 0):.1f}% complete")
                print_info(f"  Current phase: {progress.get('current_phase', 'unknown')}")
                print_info(f"  Strategy ID: {progress.get('strategy_id', 'missing')}")
                print_info(f"  ETA: {progress.get('estimated_completion_minutes', 0)} minutes")
                
                # Print full response for debugging
                print_info(f"  Full response: {json.dumps(progress, indent=2)}")
                
            else:
                print_error(f"Attempt {attempt + 1}: Progress tracking failed - {response.status_code}")
                print_error(f"  Error response: {response.text}")
                
                # This is likely where the "Failed to track progress" error occurs
                if response.status_code == 500:
                    error_detail = response.json().get("detail", "Unknown error")
                    print_error(f"  Detailed error: {error_detail}")
                    return False
                    
        except Exception as e:
            print_error(f"Attempt {attempt + 1}: Progress tracking exception - {e}")
            return False
    
    return True

def test_without_auth():
    """Test progress tracking without authentication to see if optional auth works"""
    print_info("Testing progress tracking WITHOUT authentication...")
    
    # First create a strategy with auth
    token = get_auth_token()
    if not token:
        print_error("Cannot test without auth - failed to get initial token")
        return
    
    strategy_id = test_research_endpoints_direct(token)
    if not strategy_id:
        print_error("Cannot test without auth - failed to create strategy")
        return
    
    # Execute with auth
    headers = {"Authorization": f"Bearer {token}"}
    try:
        response = requests.post(
            f"{base_url}/api/v1/research-strategy/execute/{strategy_id}",
            headers=headers,
            timeout=15
        )
        
        if response.status_code != 200:
            print_error("Failed to execute strategy for no-auth test")
            return
            
    except Exception as e:
        print_error(f"Execute error for no-auth test: {e}")
        return
    
    # Now try progress WITHOUT auth headers
    time.sleep(2)
    
    try:
        response = requests.get(
            f"{base_url}/api/v1/research-strategy/progress/{strategy_id}",
            timeout=10  # No auth headers
        )
        
        if response.status_code == 200:
            print_success("Progress tracking works WITHOUT authentication (optional auth working)")
            progress = response.json()
            print_info(f"Progress: {progress.get('progress_percentage', 0):.1f}%")
        else:
            print_error(f"Progress tracking failed without auth: {response.status_code} - {response.text}")
            
    except Exception as e:
        print_error(f"Progress tracking without auth error: {e}")

def main():
    print_header("Cat and Mouse Game Progress Tracking Debug")
    print("This script reproduces the exact user scenario to identify the root cause")
    print("User report: 'Failed to track progress' error when analyzing Cat and Mouse Game")
    
    # Check backend health
    if not test_backend_health():
        print_error("Backend is not running. Start the backend with:")
        print("  cd backend && python main.py")
        return False
    
    # Get authentication
    token = get_auth_token()
    if not token:
        print_warning("Proceeding without authentication to test optional auth")
    
    # Test the main flow
    strategy_id = test_research_endpoints_direct(token)
    if not strategy_id:
        print_error("Failed at strategy creation step")
        return False
    
    # Test execution and progress (this is where the error occurs)
    success = test_execution_and_progress(strategy_id, token)
    
    if success:
        print_success("All tests passed! Progress tracking is working correctly.")
        print_info("The 'Failed to track progress' error might be:")
        print_info("  1. A frontend issue (axios configuration)")
        print_info("  2. A timing issue (frontend polling too early)")
        print_info("  3. A CORS issue")
        print_info("  4. An authentication token issue")
    else:
        print_error("Progress tracking is failing at the backend level")
        print_info("Check the backend logs for more details")
    
    # Test optional auth
    test_without_auth()
    
    return success

if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1)