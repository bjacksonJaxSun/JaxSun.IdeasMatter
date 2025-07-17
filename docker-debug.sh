#!/bin/bash
# Docker Debug Script for Jackson Ideas
# This script helps debug Docker build issues

echo "=== Docker Debug Script ==="
echo ""

# Check if Docker is available
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed"
    echo "Please install Docker Desktop from: https://www.docker.com/products/docker-desktop"
    exit 1
fi

echo "✅ Docker version: $(docker --version)"

# Function to debug Docker build
debug_build() {
    echo ""
    echo "=== Debugging Docker Build Process ==="
    
    # Create a debug Dockerfile
    cat > Dockerfile.debug << 'EOF'
# Debug Dockerfile for Jackson Ideas Web Application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Jackson.Ideas.Web/Jackson.Ideas.Web.csproj", "Jackson.Ideas.Web/"]
COPY ["src/Jackson.Ideas.Application/Jackson.Ideas.Application.csproj", "Jackson.Ideas.Application/"]
COPY ["src/Jackson.Ideas.Core/Jackson.Ideas.Core.csproj", "Jackson.Ideas.Core/"]
COPY ["src/Jackson.Ideas.Infrastructure/Jackson.Ideas.Infrastructure.csproj", "Jackson.Ideas.Infrastructure/"]
COPY ["src/Jackson.Ideas.Shared/Jackson.Ideas.Shared.csproj", "Jackson.Ideas.Shared/"]

# Restore dependencies
RUN dotnet restore "Jackson.Ideas.Web/Jackson.Ideas.Web.csproj"

# Copy source code
COPY src/ .

# Debug: Show what we have before build
RUN echo "=== Files in /src ==="
RUN find /src -type f -name "*.csproj" -o -name "*.cs" -o -name "*.razor" -o -name "*.css" -o -name "*.png" -o -name "*.html" | head -20
RUN echo "=== Contents of Jackson.Ideas.Web directory ==="
RUN ls -la /src/Jackson.Ideas.Web/
RUN echo "=== Contents of wwwroot directory ==="
RUN ls -la /src/Jackson.Ideas.Web/wwwroot/ || echo "wwwroot directory not found"

# Build Web application
WORKDIR "/src/Jackson.Ideas.Web"
RUN dotnet publish "Jackson.Ideas.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false --verbosity normal

# Debug: Show what we have after publish
RUN echo "=== Contents of publish directory ==="
RUN ls -la /app/publish/
RUN echo "=== Contents of publish/wwwroot ==="
RUN ls -la /app/publish/wwwroot/ || echo "wwwroot directory not found in publish"
RUN echo "=== All files in publish directory ==="
RUN find /app/publish -type f | head -30

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Debug: Show what we have in final image
RUN echo "=== Contents of final /app directory ==="
RUN ls -la /app/
RUN echo "=== Contents of final wwwroot ==="
RUN ls -la /app/wwwroot/ || echo "wwwroot directory not found in final image"

# Install SQLite
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Create database directory
RUN mkdir -p /app/data && chmod 777 /app/data

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000
ENV PORT=10000
ENV UseMockAuthentication=true

EXPOSE 10000

# Start application
ENTRYPOINT ["dotnet", "Jackson.Ideas.Web.dll"]
EOF

    echo "✅ Created debug Dockerfile"
    
    # Build with debug info
    echo "Building debug image..."
    docker build -f Dockerfile.debug -t jackson-ideas-debug .
    
    if [ $? -eq 0 ]; then
        echo "✅ Debug build completed"
        echo ""
        echo "To run the debug container:"
        echo "docker run -p 10000:10000 jackson-ideas-debug"
    else
        echo "❌ Debug build failed"
        exit 1
    fi
}

# Function to inspect the image
inspect_image() {
    echo ""
    echo "=== Inspecting Docker Image ==="
    
    if ! docker image inspect jackson-ideas-debug &> /dev/null; then
        echo "❌ Debug image not found. Run 'debug-build' first."
        exit 1
    fi
    
    # Run container and inspect filesystem
    echo "Starting temporary container to inspect filesystem..."
    docker run -d --name jackson-ideas-inspect jackson-ideas-debug tail -f /dev/null
    
    echo ""
    echo "=== Files in /app directory ==="
    docker exec jackson-ideas-inspect ls -la /app/
    
    echo ""
    echo "=== Checking for wwwroot ==="
    docker exec jackson-ideas-inspect ls -la /app/wwwroot/ || echo "wwwroot directory not found"
    
    echo ""
    echo "=== Looking for CSS files ==="
    docker exec jackson-ideas-inspect find /app -name "*.css" || echo "No CSS files found"
    
    echo ""
    echo "=== Looking for static files ==="
    docker exec jackson-ideas-inspect find /app -name "*.png" -o -name "*.html" -o -name "*.js" || echo "No static files found"
    
    echo ""
    echo "=== Checking application files ==="
    docker exec jackson-ideas-inspect ls -la /app/*.dll || echo "No DLL files found"
    
    # Cleanup
    docker stop jackson-ideas-inspect
    docker rm jackson-ideas-inspect
}

# Function to test the debug container
test_debug_container() {
    echo ""
    echo "=== Testing Debug Container ==="
    
    # Start container in background
    docker run -d -p 10000:10000 \
        --name jackson-ideas-debug-test \
        jackson-ideas-debug
    
    # Wait for startup
    echo "Waiting for container to start..."
    sleep 10
    
    # Test endpoints
    echo "Testing /health endpoint..."
    curl -s http://localhost:10000/health || echo "Health endpoint failed"
    
    echo ""
    echo "Testing /app.css..."
    curl -s -I http://localhost:10000/app.css || echo "CSS file not found"
    
    echo ""
    echo "Testing / (main page)..."
    curl -s -I http://localhost:10000/ || echo "Main page not found"
    
    echo ""
    echo "Container logs:"
    docker logs jackson-ideas-debug-test
    
    # Cleanup
    docker stop jackson-ideas-debug-test
    docker rm jackson-ideas-debug-test
}

# Function to compare with working example
compare_with_working() {
    echo ""
    echo "=== Creating Working Example Dockerfile ==="
    
    # Create a minimal working example
    cat > Dockerfile.working << 'EOF'
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy everything
COPY . .

# Restore and build
RUN dotnet restore src/Jackson.Ideas.Web/Jackson.Ideas.Web.csproj
RUN dotnet publish src/Jackson.Ideas.Web/Jackson.Ideas.Web.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Jackson.Ideas.Web.dll"]
EOF

    echo "✅ Created working example Dockerfile"
    
    # Build working example
    echo "Building working example..."
    docker build -f Dockerfile.working -t jackson-ideas-working .
    
    if [ $? -eq 0 ]; then
        echo "✅ Working example built successfully"
        echo ""
        echo "To test the working example:"
        echo "docker run -p 10001:10000 jackson-ideas-working"
    else
        echo "❌ Working example build failed"
    fi
}

# Main menu
case "$1" in
    "debug-build")
        debug_build
        ;;
    "inspect")
        inspect_image
        ;;
    "test")
        test_debug_container
        ;;
    "working")
        compare_with_working
        ;;
    "full-debug")
        echo "=== Full Debug Workflow ==="
        debug_build
        inspect_image
        test_debug_container
        compare_with_working
        ;;
    *)
        echo "Usage: $0 {debug-build|inspect|test|working|full-debug}"
        echo ""
        echo "Commands:"
        echo "  debug-build  - Build Docker image with debug output"
        echo "  inspect      - Inspect the built image filesystem"
        echo "  test         - Test the debug container"
        echo "  working      - Create and test a working example"
        echo "  full-debug   - Run complete debug workflow"
        echo ""
        echo "Example:"
        echo "  ./docker-debug.sh full-debug"
        exit 1
        ;;
esac