#!/bin/bash

# Simple Golden Rule: Build → Launch → Validate → Pass
# No automatic fixing to prevent file corruption

set -e

# Configuration
PROJECT_DIR="/mnt/c/Development/Jackson.Ideas"
PROJECT_PATH="$PROJECT_DIR/src/Jackson.Ideas.Mock"
DOTNET_PATH="/home/owner/.dotnet/dotnet"
LOG_DIR="$PROJECT_DIR/logs"
PORT=5000
TIMEOUT=30

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# Create logs directory
mkdir -p "$LOG_DIR"

echo -e "${BLUE}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║                    SIMPLE GOLDEN RULE                       ║${NC}"
echo -e "${BLUE}║            Build → Launch → Validate → Success              ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Step 1: Build
echo -e "${YELLOW}[STEP 1]${NC} Building Solution..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
cd "$PROJECT_DIR"

if $DOTNET_PATH build "$PROJECT_PATH" > "$LOG_DIR/build-output.log" 2>&1; then
    echo -e "${GREEN}✓ Build SUCCESSFUL${NC}"
else
    echo -e "${RED}✗ Build FAILED${NC}"
    echo "Build errors:"
    tail -20 "$LOG_DIR/build-output.log" | grep -E "(error|Error|ERROR)" || echo "See full log at: $LOG_DIR/build-output.log"
    exit 1
fi

# Step 2: Launch Application
echo ""
echo -e "${YELLOW}[STEP 2]${NC} Launching Application..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Starting application at http://localhost:$PORT"

# Start app in background
cd "$PROJECT_PATH"
$DOTNET_PATH run --urls "http://localhost:$PORT" > "$LOG_DIR/app-output.log" 2>&1 &
APP_PID=$!

echo "Application started with PID: $APP_PID"

# Wait for app to be ready
echo "Waiting for application to be ready..."
READY=false
for i in {1..30}; do
    if curl -s "http://localhost:$PORT" > /dev/null 2>&1; then
        READY=true
        break
    fi
    echo -n "."
    sleep 1
done

if [ "$READY" = false ]; then
    echo -e "\n${RED}✗ Application failed to start within $TIMEOUT seconds${NC}"
    kill $APP_PID 2>/dev/null || true
    exit 1
fi

echo -e "\n${GREEN}✓ Application is running${NC}"

# Step 3: Validate HTML Content
echo ""
echo -e "${YELLOW}[STEP 3]${NC} Validating Application Content..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Test the home page
HTML_CONTENT=$(curl -s "http://localhost:$PORT" || echo "FAILED")

if echo "$HTML_CONTENT" | grep -qi "Ideas Matter"; then
    echo -e "${GREEN}✓ Application content validation PASSED${NC}"
    echo "  - Found 'Ideas Matter' in response"
    
    # Additional checks
    if echo "$HTML_CONTENT" | grep -qi "html"; then
        echo -e "${GREEN}✓ Valid HTML response detected${NC}"
    fi
    
    if echo "$HTML_CONTENT" | grep -qi "bootstrap\|css"; then
        echo -e "${GREEN}✓ Styling framework detected${NC}"
    fi
    
    VALIDATION_SUCCESS=true
else
    echo -e "${RED}✗ Application content validation FAILED${NC}"
    echo "Expected content not found in response"
    echo "Response preview: ${HTML_CONTENT:0:200}..."
    VALIDATION_SUCCESS=false
fi

# Cleanup
echo ""
echo -e "${YELLOW}[CLEANUP]${NC} Stopping application..."
kill $APP_PID 2>/dev/null || true
sleep 2

# Final Result
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if [ "$VALIDATION_SUCCESS" = true ]; then
    echo -e "${GREEN}🎉 GOLDEN RULE VALIDATION PASSED! 🎉${NC}"
    echo ""
    echo -e "${GREEN}Woohoo, Bobby is amazing! 🚀${NC}"
    echo ""
    echo "✅ Build: SUCCESSFUL"
    echo "✅ Launch: SUCCESSFUL" 
    echo "✅ Validation: SUCCESSFUL"
    echo ""
    echo "Application is ready for development and testing!"
else
    echo -e "${RED}💥 GOLDEN RULE VALIDATION FAILED 💥${NC}"
    echo ""
    echo "❌ Build: SUCCESSFUL"
    echo "❌ Launch: SUCCESSFUL"
    echo "❌ Validation: FAILED"
    echo ""
    echo "Check logs in: $LOG_DIR"
    exit 1
fi

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Timestamp: $(date)"
echo "Logs saved to: $LOG_DIR"