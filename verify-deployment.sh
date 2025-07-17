#!/bin/bash

# Verify Render deployment script
echo "🔍 Verifying JaxSun.IdeasMatter deployment on Render..."

# Expected URLs (update these once Render provides actual URLs)
API_URL="https://jaxsun-ideasmatter-api.onrender.com"
WEB_URL="https://jaxsun-ideasmatter-web.onrender.com"

echo ""
echo "🌐 Testing API Service..."
echo "API URL: $API_URL"

# Test API health endpoint
echo "Testing API health endpoint..."
if curl -s -f "$API_URL/health" > /dev/null; then
    echo "✅ API health endpoint is responding"
else
    echo "❌ API health endpoint is not responding"
fi

# Test API base endpoint
echo "Testing API base endpoint..."
if curl -s -f "$API_URL" > /dev/null; then
    echo "✅ API base endpoint is responding"
else
    echo "❌ API base endpoint is not responding"
fi

echo ""
echo "🌐 Testing Web Service..."
echo "Web URL: $WEB_URL"

# Test web application
echo "Testing web application..."
if curl -s -f "$WEB_URL" > /dev/null; then
    echo "✅ Web application is responding"
else
    echo "❌ Web application is not responding"
fi

# Test web health endpoint
echo "Testing web health endpoint..."
if curl -s -f "$WEB_URL/health" > /dev/null; then
    echo "✅ Web health endpoint is responding"
else
    echo "❌ Web health endpoint is not responding"
fi

echo ""
echo "📋 Deployment Verification Summary:"
echo "- API Service: $API_URL"
echo "- Web Service: $WEB_URL"
echo ""
echo "🔗 Manual Testing Steps:"
echo "1. Open web application in browser: $WEB_URL"
echo "2. Test user registration/login"
echo "3. Test idea submission process"
echo "4. Test progress tracking"
echo "5. Verify all UI components work correctly"
echo ""
echo "💡 Note: If services are not responding, they may still be building."
echo "Check Render dashboard for deployment status."