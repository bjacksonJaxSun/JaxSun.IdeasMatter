#!/bin/bash

# Authentication System Test Script
echo "🔥 Testing Authentication System..."

API_BASE="http://localhost:5002"

# Test 1: Health Check
echo "1️⃣ Testing Health Check..."
health_response=$(curl -s -w "%{http_code}" -o /tmp/health.json "$API_BASE/health")
if [ "$health_response" = "200" ]; then
    echo "✅ Health check passed"
    cat /tmp/health.json
else
    echo "❌ Health check failed (HTTP $health_response)"
fi

echo -e "\n"

# Test 2: User Registration
echo "2️⃣ Testing User Registration..."
register_data='{
  "email": "test@example.com",
  "name": "Test User",
  "password": "TestPassword123",
  "confirmPassword": "TestPassword123"
}'

register_response=$(curl -s -w "%{http_code}" -o /tmp/register.json \
  -H "Content-Type: application/json" \
  -X POST \
  -d "$register_data" \
  "$API_BASE/api/v1/auth/register")

if [ "$register_response" = "200" ]; then
    echo "✅ Registration successful"
    access_token=$(cat /tmp/register.json | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
    refresh_token=$(cat /tmp/register.json | grep -o '"refreshToken":"[^"]*"' | cut -d'"' -f4)
    echo "🔑 Access Token: ${access_token:0:50}..."
    echo "🔄 Refresh Token: ${refresh_token:0:50}..."
else
    echo "❌ Registration failed (HTTP $register_response)"
    cat /tmp/register.json
fi

echo -e "\n"

# Test 3: User Login
echo "3️⃣ Testing User Login..."
login_data='{
  "email": "test@example.com",
  "password": "TestPassword123",
  "rememberMe": false
}'

login_response=$(curl -s -w "%{http_code}" -o /tmp/login.json \
  -H "Content-Type: application/json" \
  -X POST \
  -d "$login_data" \
  "$API_BASE/api/v1/auth/login")

if [ "$login_response" = "200" ]; then
    echo "✅ Login successful"
    access_token=$(cat /tmp/login.json | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
    echo "🔑 New Access Token: ${access_token:0:50}..."
else
    echo "❌ Login failed (HTTP $login_response)"
    cat /tmp/login.json
fi

echo -e "\n"

# Test 4: Protected Endpoint (without token)
echo "4️⃣ Testing Protected Endpoint (without token)..."
unauth_response=$(curl -s -w "%{http_code}" -o /tmp/unauth.json \
  "$API_BASE/api/v1/researchsession")

if [ "$unauth_response" = "401" ]; then
    echo "✅ Properly blocked unauthorized access (HTTP 401)"
else
    echo "❌ Should have returned 401 but got HTTP $unauth_response"
fi

echo -e "\n"

# Test 5: Protected Endpoint (with token)
if [ -n "$access_token" ]; then
    echo "5️⃣ Testing Protected Endpoint (with token)..."
    auth_response=$(curl -s -w "%{http_code}" -o /tmp/auth.json \
      -H "Authorization: Bearer $access_token" \
      "$API_BASE/api/v1/researchsession")
    
    if [ "$auth_response" = "200" ]; then
        echo "✅ Authorized access successful"
    else
        echo "❌ Authorized access failed (HTTP $auth_response)"
        cat /tmp/auth.json
    fi
else
    echo "⏭️ Skipping protected endpoint test - no access token"
fi

echo -e "\n🎯 Authentication System Test Complete!"