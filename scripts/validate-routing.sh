#!/bin/bash

# Routing and Layout Validation Script
# This script validates that all routing and layout configurations are intact

echo "🔍 Validating routing and layout configurations..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

ERRORS=0

# Function to check if a file contains a pattern
check_file_pattern() {
    local file="$1"
    local pattern="$2" 
    local description="$3"
    
    if [ ! -f "$file" ]; then
        echo -e "${RED}❌ File not found: $file${NC}"
        ((ERRORS++))
        return 1
    fi
    
    if grep -q "$pattern" "$file"; then
        echo -e "${GREEN}✅ $description${NC}"
        return 0
    else
        echo -e "${RED}❌ Missing: $description in $file${NC}"
        ((ERRORS++))
        return 1
    fi
}

echo ""
echo "📄 Checking page route directives..."

# Check Home page has landing layout
check_file_pattern "src/Jackson.Ideas.Web/Components/Pages/Home.razor" "@layout Layout.LandingLayout" "Home page uses LandingLayout"
check_file_pattern "src/Jackson.Ideas.Web/Components/Pages/Home.razor" "@page \"/\"" "Home page has root route"

# Check Dashboard page has route
check_file_pattern "src/Jackson.Ideas.Web/Components/Pages/Dashboard.razor" "@page \"/dashboard\"" "Dashboard page has route directive"

# Check NewIdea page has route  
check_file_pattern "src/Jackson.Ideas.Web/Components/Pages/NewIdea.razor" "@page \"/new-idea\"" "NewIdea page has route directive"

echo ""
echo "🔧 Checking service registrations..."

# Check NavigationState service registration
check_file_pattern "src/Jackson.Ideas.Web/Program.cs" "NavigationState" "NavigationState service is registered"

# Check authentication service registration
check_file_pattern "src/Jackson.Ideas.Web/Program.cs" "AuthenticationStateProvider" "AuthenticationStateProvider is registered"

echo ""
echo "🎨 Checking layout files exist..."

# Check layout files exist
check_file_pattern "src/Jackson.Ideas.Web/Components/Layout/LandingLayout.razor" "@inherits LayoutComponentBase" "LandingLayout exists and is valid"
check_file_pattern "src/Jackson.Ideas.Web/Components/Layout/MainLayout.razor" "@inherits LayoutComponentBase" "MainLayout exists and is valid"
check_file_pattern "src/Jackson.Ideas.Web/Components/Layout/NavMenu.razor" "navbar-brand" "NavMenu exists"

echo ""
echo "🧪 Running build test..."

# Test build
if dotnet build Jackson.Ideas.sln --verbosity quiet > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Solution builds successfully${NC}"
else
    echo -e "${RED}❌ Build failed - run 'dotnet build' for details${NC}"
    ((ERRORS++))
fi

echo ""
echo "📊 Validation Summary"
echo "===================="

if [ $ERRORS -eq 0 ]; then
    echo -e "${GREEN}🎉 All routing and layout validations passed!${NC}"
    echo -e "${GREEN}   Your routing configuration is intact and protected.${NC}"
    exit 0
else
    echo -e "${RED}❌ Found $ERRORS issue(s) with routing/layout configuration${NC}"
    echo -e "${YELLOW}   Please fix the issues above and run again.${NC}"
    echo -e "${YELLOW}   Refer to docs/ROUTING_AND_LAYOUTS.md for guidance.${NC}"
    exit 1
fi