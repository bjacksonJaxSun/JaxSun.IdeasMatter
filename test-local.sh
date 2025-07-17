#!/bin/bash
# Test script to validate all fixes before deployment

echo "=== Jackson Ideas Local Test Script ==="
echo "This script tests all fixes before deployment to Render"
echo ""

# Check prerequisites
echo "Checking prerequisites..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 9 SDK"
    exit 1
fi

if ! command -v docker &> /dev/null; then
    echo "❌ Docker not found. Please install Docker Desktop"
    exit 1
fi

echo "✅ Prerequisites check passed"
echo ""

# Function to test project configuration
test_project_config() {
    echo "=== Testing Project Configuration ==="
    
    # Check if wwwroot exists
    if [ ! -d "src/Jackson.Ideas.Web/wwwroot" ]; then
        echo "❌ wwwroot directory not found"
        exit 1
    fi
    
    # Check if static files exist
    if [ ! -f "src/Jackson.Ideas.Web/wwwroot/app.css" ]; then
        echo "❌ app.css not found"
        exit 1
    fi
    
    if [ ! -f "src/Jackson.Ideas.Web/wwwroot/favicon.png" ]; then
        echo "❌ favicon.png not found"
        exit 1
    fi
    
    echo "✅ wwwroot directory and static files exist"
    
    # Check project file configuration
    if grep -q "StaticWebAssetsEnabled" src/Jackson.Ideas.Web/Jackson.Ideas.Web.csproj; then
        echo "✅ StaticWebAssetsEnabled found in project file"
    else
        echo "❌ StaticWebAssetsEnabled not found in project file"
        exit 1
    fi
    
    if grep -q "Content Include" src/Jackson.Ideas.Web/Jackson.Ideas.Web.csproj; then
        echo "✅ Content Include configuration found"
    else
        echo "❌ Content Include configuration not found"
        exit 1
    fi
    
    echo "✅ Project configuration test passed"
}

# Function to test local development
test_local_dev() {
    echo ""
    echo "=== Testing Local Development ==="
    
    # Start the application in background
    echo "Starting application locally..."
    cd src/Jackson.Ideas.Web
    
    # Set environment variables
    export ASPNETCORE_ENVIRONMENT=Development
    export ConnectionStrings__DefaultConnection="Data Source=test.db"
    export UseMockAuthentication=true
    export DemoMode__Enabled=true
    export DemoMode__UseRealAI=false
    
    # Start application in background
    dotnet run --urls "http://localhost:5000" > /tmp/app.log 2>&1 &
    APP_PID=$!
    cd ../..
    
    # Wait for application to start
    echo "Waiting for application to start..."
    sleep 15
    
    # Test endpoints
    echo "Testing local endpoints..."
    
    # Test health endpoint
    if curl -s -f http://localhost:5000/health > /dev/null; then
        echo "✅ Health endpoint works"
    else
        echo "❌ Health endpoint failed"
        kill $APP_PID
        exit 1
    fi
    
    # Test static files
    if curl -s -f http://localhost:5000/app.css > /dev/null; then
        echo "✅ CSS file serves correctly"
    else
        echo "❌ CSS file not found"
        kill $APP_PID
        exit 1
    fi
    
    if curl -s -f http://localhost:5000/favicon.png > /dev/null; then
        echo "✅ Favicon serves correctly"
    else
        echo "❌ Favicon not found"
        kill $APP_PID
        exit 1
    fi
    
    # Test main page
    if curl -s -f http://localhost:5000/ > /dev/null; then
        echo "✅ Main page loads correctly"
    else
        echo "❌ Main page failed to load"
        kill $APP_PID
        exit 1
    fi
    
    # Stop application
    kill $APP_PID
    echo "✅ Local development test passed"
}

# Function to test Docker build
test_docker_build() {
    echo ""
    echo "=== Testing Docker Build ==="
    
    # Build Docker image
    echo "Building Docker image..."
    docker build -f Dockerfile.render -t jackson-ideas-test . > /tmp/docker_build.log 2>&1
    
    if [ $? -eq 0 ]; then
        echo "✅ Docker build succeeded"
    else
        echo "❌ Docker build failed"
        echo "Build log:"
        cat /tmp/docker_build.log
        exit 1
    fi
    
    # Start Docker container
    echo "Starting Docker container..."
    docker run -d -p 10000:10000 \
        -e ASPNETCORE_ENVIRONMENT=Production \
        -e ConnectionStrings__DefaultConnection="Data Source=/app/data/test.db" \
        -e UseMockAuthentication=true \
        -e DemoMode__Enabled=true \
        -e DemoMode__UseRealAI=false \
        --name jackson-ideas-test \
        jackson-ideas-test > /tmp/docker_run.log 2>&1
    
    if [ $? -eq 0 ]; then
        echo "✅ Docker container started"
    else
        echo "❌ Docker container failed to start"
        echo "Run log:"
        cat /tmp/docker_run.log
        exit 1
    fi
    
    # Wait for container to be ready
    echo "Waiting for container to be ready..."
    sleep 15
    
    # Test Docker endpoints
    echo "Testing Docker endpoints..."
    
    # Test health endpoint
    if curl -s -f http://localhost:10000/health > /dev/null; then
        echo "✅ Docker health endpoint works"
    else
        echo "❌ Docker health endpoint failed"
        docker logs jackson-ideas-test
        docker stop jackson-ideas-test
        docker rm jackson-ideas-test
        exit 1
    fi
    
    # Test static files
    if curl -s -f http://localhost:10000/app.css > /dev/null; then
        echo "✅ Docker CSS file serves correctly"
    else
        echo "❌ Docker CSS file not found"
        docker logs jackson-ideas-test
        docker stop jackson-ideas-test
        docker rm jackson-ideas-test
        exit 1
    fi
    
    if curl -s -f http://localhost:10000/favicon.png > /dev/null; then
        echo "✅ Docker favicon serves correctly"
    else
        echo "❌ Docker favicon not found"
        docker logs jackson-ideas-test
        docker stop jackson-ideas-test
        docker rm jackson-ideas-test
        exit 1
    fi
    
    # Test main page
    if curl -s -f http://localhost:10000/ > /dev/null; then
        echo "✅ Docker main page loads correctly"
    else
        echo "❌ Docker main page failed to load"
        docker logs jackson-ideas-test
        docker stop jackson-ideas-test
        docker rm jackson-ideas-test
        exit 1
    fi
    
    # Cleanup
    docker stop jackson-ideas-test
    docker rm jackson-ideas-test
    
    echo "✅ Docker build and test passed"
}

# Function to run all tests
run_all_tests() {
    echo "=== Running All Tests ==="
    test_project_config
    test_local_dev
    test_docker_build
    echo ""
    echo "🎉 All tests passed! Ready for deployment to Render."
}

# Main execution
case "$1" in
    "config")
        test_project_config
        ;;
    "local")
        test_local_dev
        ;;
    "docker")
        test_docker_build
        ;;
    "all"|"")
        run_all_tests
        ;;
    *)
        echo "Usage: $0 {config|local|docker|all}"
        echo ""
        echo "Commands:"
        echo "  config - Test project configuration"
        echo "  local  - Test local development"
        echo "  docker - Test Docker build and run"
        echo "  all    - Run all tests (default)"
        exit 1
        ;;
esac