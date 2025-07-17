#!/bin/bash
# Local Development Script for Jackson Ideas
# This script helps you run and test the application locally

echo "=== Jackson Ideas Local Development Script ==="
echo ""

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed"
    echo "Please install .NET 9 SDK from: https://dotnet.microsoft.com/download/dotnet/9.0"
    exit 1
fi

# Check .NET version
echo "✅ .NET SDK version: $(dotnet --version)"

# Set local development environment variables
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Data Source=jackson_ideas_dev.db"
export UseMockAuthentication=true
export DemoMode__Enabled=true
export DemoMode__UseRealAI=false

echo "✅ Environment variables set for local development"

# Function to run the application
run_app() {
    echo ""
    echo "=== Running Jackson Ideas Web Application ==="
    echo "🌐 Application will be available at: http://localhost:5000"
    echo "📱 Or: https://localhost:5001"
    echo ""
    echo "Press Ctrl+C to stop the application"
    echo ""
    
    cd src/Jackson.Ideas.Web
    dotnet run --urls "http://localhost:5000;https://localhost:5001"
}

# Function to test static files
test_static_files() {
    echo ""
    echo "=== Testing Static Files ==="
    echo "Testing if static files are accessible..."
    
    # Wait for app to start
    sleep 3
    
    echo "Testing http://localhost:5000/app.css"
    curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/app.css
    echo ""
    
    echo "Testing http://localhost:5000/favicon.png"
    curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/favicon.png
    echo ""
    
    echo "Testing http://localhost:5000/"
    curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/
    echo ""
}

# Function to build Docker image locally
build_docker() {
    echo ""
    echo "=== Building Docker Image Locally ==="
    echo "Building jackson-ideas-web image..."
    
    docker build -f Dockerfile.render -t jackson-ideas-web .
    
    if [ $? -eq 0 ]; then
        echo "✅ Docker image built successfully"
        echo ""
        echo "To run the container:"
        echo "docker run -p 10000:10000 jackson-ideas-web"
    else
        echo "❌ Docker build failed"
        exit 1
    fi
}

# Function to run Docker container locally
run_docker() {
    echo ""
    echo "=== Running Docker Container Locally ==="
    echo "🐳 Container will be available at: http://localhost:10000"
    echo ""
    echo "Press Ctrl+C to stop the container"
    echo ""
    
    docker run -p 10000:10000 \
        -e ASPNETCORE_ENVIRONMENT=Production \
        -e ConnectionStrings__DefaultConnection="Data Source=/app/data/jackson_ideas.db" \
        -e UseMockAuthentication=true \
        -e DemoMode__Enabled=true \
        -e DemoMode__UseRealAI=false \
        jackson-ideas-web
}

# Function to test Docker container
test_docker() {
    echo ""
    echo "=== Testing Docker Container ==="
    echo "Testing if Docker container serves files correctly..."
    
    # Wait for container to start
    sleep 5
    
    echo "Testing http://localhost:10000/health"
    curl -s http://localhost:10000/health | jq '.' 2>/dev/null || curl -s http://localhost:10000/health
    echo ""
    
    echo "Testing http://localhost:10000/app.css"
    curl -s -o /dev/null -w "%{http_code}" http://localhost:10000/app.css
    echo ""
    
    echo "Testing http://localhost:10000/"
    curl -s -o /dev/null -w "%{http_code}" http://localhost:10000/
    echo ""
}

# Main menu
case "$1" in
    "run")
        run_app
        ;;
    "test-static")
        test_static_files
        ;;
    "build-docker")
        build_docker
        ;;
    "run-docker")
        run_docker
        ;;
    "test-docker")
        test_docker
        ;;
    "full-test")
        echo "=== Full Local Testing Workflow ==="
        echo "1. Building Docker image..."
        build_docker
        echo ""
        echo "2. Starting Docker container in background..."
        docker run -d -p 10000:10000 \
            -e ASPNETCORE_ENVIRONMENT=Production \
            -e ConnectionStrings__DefaultConnection="Data Source=/app/data/jackson_ideas.db" \
            -e UseMockAuthentication=true \
            -e DemoMode__Enabled=true \
            -e DemoMode__UseRealAI=false \
            --name jackson-ideas-test \
            jackson-ideas-web
        echo ""
        echo "3. Testing Docker container..."
        test_docker
        echo ""
        echo "4. Stopping test container..."
        docker stop jackson-ideas-test
        docker rm jackson-ideas-test
        echo "✅ Full test completed"
        ;;
    *)
        echo "Usage: $0 {run|test-static|build-docker|run-docker|test-docker|full-test}"
        echo ""
        echo "Commands:"
        echo "  run          - Run the application locally with dotnet run"
        echo "  test-static  - Test static file serving (run this in another terminal while app is running)"
        echo "  build-docker - Build Docker image locally"
        echo "  run-docker   - Run Docker container locally"
        echo "  test-docker  - Test Docker container (run this in another terminal while container is running)"
        echo "  full-test    - Complete automated test workflow"
        echo ""
        echo "Examples:"
        echo "  ./local-dev.sh run"
        echo "  ./local-dev.sh full-test"
        exit 1
        ;;
esac