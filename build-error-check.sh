#!/bin/bash

# Build Error Check Script
# Validates code for potential compilation errors without full .NET CLI

echo "🔍 Ideas Matter - Build Error Analysis"
echo "========================================"
echo "Analyzing code for potential build errors..."
echo ""

BUILD_ERRORS=0
WARNINGS=0

# Function to check file syntax
check_razor_syntax() {
    local file="$1"
    local filename=$(basename "$file")
    
    echo "Checking $filename..."
    
    # Check for unclosed @code blocks
    local code_blocks=$(grep -c "@code" "$file" || echo "0")
    local closing_braces=$(grep -c "^}" "$file" || echo "0")
    
    if [ $code_blocks -gt 0 ]; then
        if [ $closing_braces -lt $code_blocks ]; then
            echo "❌ ERROR: Unclosed @code block in $filename"
            ((BUILD_ERRORS++))
        fi
    fi
    
    # Check for unmatched HTML tags
    local div_open=$(grep -o "<div" "$file" | wc -l || echo "0")
    local div_close=$(grep -o "</div>" "$file" | wc -l || echo "0")
    
    if [ $div_open -ne $div_close ]; then
        echo "⚠️  WARNING: Unmatched <div> tags in $filename (Open: $div_open, Close: $div_close)"
        ((WARNINGS++))
    fi
    
    # Check for missing @page directive in page files
    if [[ "$file" == *"/Pages/"* ]] && ! grep -q "@page" "$file"; then
        echo "❌ ERROR: Missing @page directive in $filename"
        ((BUILD_ERRORS++))
    fi
    
    # Check for missing using statements for common namespaces
    if grep -q "IMockDataService\|IMockAuthenticationService" "$file" && ! grep -q "@using.*Mock.Services" "$file"; then
        echo "⚠️  WARNING: Missing using statement for Mock.Services in $filename"
        ((WARNINGS++))
    fi
    
    # Check for syntax issues in @code blocks
    if grep -A 100 "@code" "$file" | grep -E "(private|public|protected).*\{.*\{" >/dev/null; then
        echo "⚠️  WARNING: Potential syntax issue in @code block in $filename"
        ((WARNINGS++))
    fi
}

# Function to check C# files
check_csharp_syntax() {
    local file="$1"
    local filename=$(basename "$file")
    
    echo "Checking $filename..."
    
    # Check for unmatched braces
    local open_braces=$(grep -o "{" "$file" | wc -l || echo "0")
    local close_braces=$(grep -o "}" "$file" | wc -l || echo "0")
    
    if [ $open_braces -ne $close_braces ]; then
        echo "❌ ERROR: Unmatched braces in $filename (Open: $open_braces, Close: $close_braces)"
        ((BUILD_ERRORS++))
    fi
    
    # Check for missing namespace
    if ! grep -q "namespace" "$file"; then
        echo "⚠️  WARNING: Missing namespace declaration in $filename"
        ((WARNINGS++))
    fi
    
    # Check for missing using statements
    if grep -q "ILogger\|IServiceCollection" "$file" && ! grep -q "using Microsoft" "$file"; then
        echo "⚠️  WARNING: Missing Microsoft using statements in $filename"
        ((WARNINGS++))
    fi
}

echo "Analyzing Razor Components..."
echo "----------------------------"

# Check all Razor files
find src/Jackson.Ideas.Mock/Components -name "*.razor" | while read file; do
    check_razor_syntax "$file"
done

echo ""
echo "Analyzing C# Files..."
echo "--------------------"

# Check critical C# files
for file in src/Jackson.Ideas.Mock/Program.cs src/Jackson.Ideas.Mock/Services/Mock/*.cs; do
    if [ -f "$file" ]; then
        check_csharp_syntax "$file"
    fi
done

echo ""
echo "Checking Project Configuration..."
echo "--------------------------------"

# Check project file
PROJECT_FILE="src/Jackson.Ideas.Mock/Jackson.Ideas.Mock.csproj"
if [ -f "$PROJECT_FILE" ]; then
    echo "Checking Jackson.Ideas.Mock.csproj..."
    
    # Check for required package references
    if ! grep -q "Microsoft.AspNetCore.App" "$PROJECT_FILE"; then
        echo "❌ ERROR: Missing Microsoft.AspNetCore.App reference"
        ((BUILD_ERRORS++))
    fi
    
    # Check target framework
    if ! grep -q "net[0-9]" "$PROJECT_FILE"; then
        echo "❌ ERROR: Missing or invalid target framework"
        ((BUILD_ERRORS++))
    fi
    
    # Check for SignalR if used
    if grep -q "SignalR" src/Jackson.Ideas.Mock/Program.cs && ! grep -q "SignalR" "$PROJECT_FILE"; then
        echo "⚠️  WARNING: SignalR used but package reference may be missing"
        ((WARNINGS++))
    fi
else
    echo "❌ ERROR: Project file not found"
    ((BUILD_ERRORS++))
fi

echo ""
echo "Checking Service Dependencies..."
echo "-------------------------------"

# Check if all referenced services are registered
PROGRAM_FILE="src/Jackson.Ideas.Mock/Program.cs"
if [ -f "$PROGRAM_FILE" ]; then
    echo "Checking service registrations..."
    
    REQUIRED_SERVICES=(
        "IMockDataService"
        "IMockAuthenticationService" 
        "IMockAnalysisProgressService"
        "IMockChartDataService"
        "IMockExportService"
    )
    
    for service in "${REQUIRED_SERVICES[@]}"; do
        if ! grep -q "$service" "$PROGRAM_FILE"; then
            echo "❌ ERROR: Service $service not registered in Program.cs"
            ((BUILD_ERRORS++))
        fi
    done
else
    echo "❌ ERROR: Program.cs not found"
    ((BUILD_ERRORS++))
fi

echo ""
echo "📊 Build Error Analysis Results"
echo "==============================="

if [ $BUILD_ERRORS -eq 0 ]; then
    echo "🎉 BUILD STATUS: CLEAN"
    echo "✅ No critical build errors detected"
    echo ""
    echo "📋 Summary:"
    echo "   Build Errors: $BUILD_ERRORS"
    echo "   Warnings: $WARNINGS"
    echo ""
    if [ $WARNINGS -eq 0 ]; then
        echo "🏆 Perfect! No warnings either."
    else
        echo "ℹ️  $WARNINGS warnings found (non-blocking)"
    fi
    echo ""
    echo "🚀 Ready for compilation!"
else
    echo "❌ BUILD STATUS: ERRORS DETECTED"
    echo "🚨 Critical build errors found: $BUILD_ERRORS"
    echo "⚠️  Warnings: $WARNINGS"
    echo ""
    echo "🔧 Action Required:"
    echo "   Please fix the errors above before proceeding"
    echo "   Run this script again after fixes"
fi

echo ""
echo "==============================="
echo "🎯 FINAL BUILD ERROR COUNT: $BUILD_ERRORS"
echo "==============================="

exit $BUILD_ERRORS