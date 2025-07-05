#!/bin/bash
# Build verification script for Jackson.Ideas solution

echo "=== Jackson.Ideas Build Verification ==="
echo "Checking solution structure..."

# Check if solution file exists
if [ ! -f "Jackson.Ideas.sln" ]; then
    echo "❌ Solution file not found"
    exit 1
fi

echo "✅ Solution file found"

# Check if dotnet is available
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8 SDK"
    exit 1
fi

echo "✅ .NET SDK available"

# Check project files
echo "Checking project files..."
projects=(
    "src/Jackson.Ideas.Api/Jackson.Ideas.Api.csproj"
    "src/Jackson.Ideas.Application/Jackson.Ideas.Application.csproj"
    "src/Jackson.Ideas.Core/Jackson.Ideas.Core.csproj"
    "src/Jackson.Ideas.Infrastructure/Jackson.Ideas.Infrastructure.csproj"
    "src/Jackson.Ideas.Shared/Jackson.Ideas.Shared.csproj"
    "src/Jackson.Ideas.Web/Jackson.Ideas.Web.csproj"
)

for project in "${projects[@]}"; do
    if [ ! -f "$project" ]; then
        echo "❌ Project file not found: $project"
        exit 1
    fi
    echo "✅ Found: $project"
done

echo "Checking test projects..."
test_projects=(
    "tests/Jackson.Ideas.Api.Tests/Jackson.Ideas.Api.Tests.csproj"
    "tests/Jackson.Ideas.Application.Tests/Jackson.Ideas.Application.Tests.csproj"
    "tests/Jackson.Ideas.Core.Tests/Jackson.Ideas.Core.Tests.csproj"
    "tests/Jackson.Ideas.Infrastructure.Tests/Jackson.Ideas.Infrastructure.Tests.csproj"
)

for project in "${test_projects[@]}"; do
    if [ ! -f "$project" ]; then
        echo "❌ Test project not found: $project"
        exit 1
    fi
    echo "✅ Found: $project"
done

echo "Attempting to restore packages..."
dotnet restore Jackson.Ideas.sln

if [ $? -ne 0 ]; then
    echo "❌ Package restore failed"
    exit 1
fi

echo "✅ Package restore successful"

echo "Attempting to build solution..."
dotnet build Jackson.Ideas.sln --configuration Debug --verbosity normal

if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

echo "✅ Build successful"

echo "Attempting to run tests..."
dotnet test Jackson.Ideas.sln --configuration Debug --verbosity normal

if [ $? -ne 0 ]; then
    echo "❌ Tests failed"
    exit 1
fi

echo "✅ All tests passed"

echo "=== Build Verification Complete ==="
echo "✅ Solution builds successfully"
echo "✅ All tests pass"
echo "✅ Ready for development"