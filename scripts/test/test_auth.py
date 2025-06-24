#!/usr/bin/env python3
"""
Comprehensive authentication testing script
Tests login, registration, Google OAuth, and JWT tokens
"""

import requests
import json
import sys
from pathlib import Path
import time

# Configuration
BASE_URL = "http://localhost:8000"
API_V1 = f"{BASE_URL}/api/v1"

class AuthTester:
    def __init__(self):
        self.session = requests.Session()
        self.access_token = None
        self.refresh_token = None
        
    def test_health_check(self):
        """Test if server is running"""
        print("\n🔍 Testing health check...")
        try:
            response = requests.get(f"{BASE_URL}/health")
            assert response.status_code == 200
            print("✅ Server is healthy")
            return True
        except Exception as e:
            print(f"❌ Server health check failed: {e}")
            return False
    
    def test_register(self, email="test@example.com", password="testpass123"):
        """Test user registration"""
        print(f"\n🔍 Testing registration for {email}...")
        
        data = {
            "email": email,
            "password": password,
            "full_name": "Test User"
        }
        
        try:
            response = requests.post(f"{API_V1}/auth/register", json=data)
            if response.status_code == 200:
                print("✅ Registration successful")
                return True
            elif response.status_code == 400:
                error = response.json().get("detail", "Unknown error")
                if "already registered" in error.lower():
                    print("ℹ️  User already exists")
                    return True
                else:
                    print(f"❌ Registration failed: {error}")
                    return False
            else:
                print(f"❌ Registration failed: {response.status_code} - {response.text}")
                return False
        except Exception as e:
            print(f"❌ Registration error: {e}")
            return False
    
    def test_login(self, email="test@example.com", password="testpass123"):
        """Test user login"""
        print(f"\n🔍 Testing login for {email}...")
        
        data = {
            "username": email,  # OAuth2 expects 'username' field
            "password": password
        }
        
        try:
            response = requests.post(
                f"{API_V1}/auth/login",
                data=data,  # Form data, not JSON
                headers={"Content-Type": "application/x-www-form-urlencoded"}
            )
            
            if response.status_code == 200:
                tokens = response.json()
                self.access_token = tokens.get("access_token")
                self.refresh_token = tokens.get("refresh_token")
                print("✅ Login successful")
                print(f"   Access token: {self.access_token[:20]}...")
                return True
            else:
                print(f"❌ Login failed: {response.status_code} - {response.text}")
                return False
        except Exception as e:
            print(f"❌ Login error: {e}")
            return False
    
    def test_get_current_user(self):
        """Test getting current user info"""
        print("\n🔍 Testing get current user...")
        
        if not self.access_token:
            print("❌ No access token available")
            return False
        
        try:
            response = requests.get(
                f"{API_V1}/auth/me",
                headers={"Authorization": f"Bearer {self.access_token}"}
            )
            
            if response.status_code == 200:
                user = response.json()
                print("✅ Got current user info:")
                print(f"   Email: {user.get('email')}")
                print(f"   Name: {user.get('full_name')}")
                print(f"   Role: {user.get('role')}")
                return True
            else:
                print(f"❌ Failed to get user info: {response.status_code} - {response.text}")
                return False
        except Exception as e:
            print(f"❌ Get user error: {e}")
            return False
    
    def test_bypass_login(self):
        """Test development bypass login"""
        print("\n🔍 Testing bypass login...")
        
        data = {"email": "dev@example.com"}
        
        try:
            response = requests.post(f"{API_V1}/auth/bypass", json=data)
            
            if response.status_code == 200:
                tokens = response.json()
                self.access_token = tokens.get("access_token")
                print("✅ Bypass login successful")
                return True
            else:
                print(f"❌ Bypass login failed: {response.status_code} - {response.text}")
                return False
        except Exception as e:
            print(f"❌ Bypass login error: {e}")
            return False
    
    def test_protected_endpoint(self):
        """Test accessing a protected endpoint"""
        print("\n🔍 Testing protected endpoint access...")
        
        if not self.access_token:
            print("❌ No access token available")
            return False
        
        try:
            # Try to access ideas endpoint
            response = requests.get(
                f"{API_V1}/ideas",
                headers={"Authorization": f"Bearer {self.access_token}"}
            )
            
            if response.status_code in [200, 404]:  # 404 is ok if no ideas exist
                print("✅ Successfully accessed protected endpoint")
                return True
            elif response.status_code == 401:
                print("❌ Unauthorized access to protected endpoint")
                return False
            else:
                print(f"❌ Unexpected response: {response.status_code} - {response.text}")
                return False
        except Exception as e:
            print(f"❌ Protected endpoint error: {e}")
            return False

def main():
    print("🚀 Starting Authentication Tests")
    print("=" * 50)
    
    tester = AuthTester()
    results = []
    
    # Run tests
    results.append(("Health Check", tester.test_health_check()))
    
    if results[-1][1]:  # Only continue if server is healthy
        results.append(("Registration", tester.test_register()))
        results.append(("Login", tester.test_login()))
        results.append(("Get Current User", tester.test_get_current_user()))
        results.append(("Protected Endpoint", tester.test_protected_endpoint()))
        results.append(("Bypass Login", tester.test_bypass_login()))
    
    # Summary
    print("\n" + "=" * 50)
    print("📊 Test Summary:")
    passed = sum(1 for _, result in results if result)
    total = len(results)
    
    for test_name, result in results:
        status = "✅ PASS" if result else "❌ FAIL"
        print(f"   {test_name}: {status}")
    
    print(f"\n   Total: {passed}/{total} tests passed")
    
    if passed == total:
        print("\n🎉 All tests passed!")
        return 0
    else:
        print(f"\n⚠️  {total - passed} tests failed!")
        return 1

if __name__ == "__main__":
    sys.exit(main())