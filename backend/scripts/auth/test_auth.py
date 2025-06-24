#!/usr/bin/env python3
"""Test authentication endpoints"""
import requests
import json

BASE_URL = "http://localhost:8000/api/v1"

print("Testing Authentication System...")
print("="*50)

# Test 1: Register a new user
print("\n1. Testing user registration...")
register_data = {
    "email": "test@example.com",
    "password": "testpassword123",
    "name": "Test User"
}

try:
    response = requests.post(f"{BASE_URL}/auth/register", json=register_data)
    print(f"Status: {response.status_code}")
    if response.status_code == 200:
        print("✅ Registration successful!")
        print(f"Response: {json.dumps(response.json(), indent=2)}")
    else:
        print("❌ Registration failed!")
        print(f"Error: {response.text}")
except Exception as e:
    print(f"❌ Request failed: {e}")

# Test 2: Login with the registered user
print("\n2. Testing login...")
# OAuth2PasswordRequestForm expects form data
login_data = {
    "username": "test@example.com",  # OAuth2 uses 'username' for email
    "password": "testpassword123"
}

try:
    # Send as form data, not JSON
    response = requests.post(
        f"{BASE_URL}/auth/login", 
        data=login_data,  # form data
        headers={"Content-Type": "application/x-www-form-urlencoded"}
    )
    print(f"Status: {response.status_code}")
    if response.status_code == 200:
        print("✅ Login successful!")
        token_data = response.json()
        print(f"Access Token: {token_data.get('access_token')[:50]}...")
    else:
        print("❌ Login failed!")
        print(f"Error: {response.text}")
except Exception as e:
    print(f"❌ Request failed: {e}")

# Test 3: Check API docs
print("\n3. Checking API documentation...")
try:
    response = requests.get("http://localhost:8000/docs")
    if response.status_code == 200:
        print("✅ API docs available at: http://localhost:8000/docs")
    else:
        print("❌ API docs not accessible")
except Exception as e:
    print(f"❌ Cannot reach API: {e}")