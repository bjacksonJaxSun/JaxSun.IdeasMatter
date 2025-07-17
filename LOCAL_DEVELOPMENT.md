# Local Development Guide

This guide helps you set up and test the Jackson Ideas application locally before deploying to Render.

## Prerequisites

### Required Software
1. **.NET 9 SDK** - Download from [Microsoft .NET Downloads](https://dotnet.microsoft.com/download/dotnet/9.0)
2. **Docker** - Download from [Docker Desktop](https://www.docker.com/products/docker-desktop)
3. **Git** - For version control
4. **curl** - For testing HTTP endpoints (usually pre-installed)

### Verify Installation
```bash
# Check .NET version (should be 9.0.x)
dotnet --version

# Check Docker
docker --version

# Check Git
git --version
```

## Quick Start

### 1. Clone and Setup
```bash
git clone <repository-url>
cd Jackson.Ideas
```

### 2. Run Application Locally
```bash
# Make script executable (Linux/Mac)
chmod +x local-dev.sh

# Run the application
./local-dev.sh run
```

The application will be available at:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001

### 3. Test Static Files
Open another terminal and run:
```bash
./local-dev.sh test-static
```

This will test if CSS, favicon, and main page are accessible.

## Docker Testing

### Build and Test Docker Container
```bash
# Build Docker image locally
./local-dev.sh build-docker

# Run Docker container
./local-dev.sh run-docker
```

The Docker container will be available at:
- **HTTP**: http://localhost:10000

### Test Docker Container
Open another terminal and run:
```bash
./local-dev.sh test-docker
```

### Full Automated Test
```bash
# Run complete test workflow
./local-dev.sh full-test
```

This will:
1. Build the Docker image
2. Start the container
3. Test all endpoints
4. Stop and clean up

## Manual Testing

### Test Static Files
```bash
# Test CSS file
curl -I http://localhost:5000/app.css

# Test favicon
curl -I http://localhost:5000/favicon.png

# Test main page
curl -I http://localhost:5000/

# Test health endpoint
curl http://localhost:5000/health
```

### Expected Responses
- **Static files**: Should return `200 OK` with appropriate content-type
- **Main page**: Should return `200 OK` with HTML content
- **Health endpoint**: Should return `200 OK` with JSON status

## Troubleshooting

### Common Issues

#### 1. .NET SDK Not Found
```bash
# Install .NET 9 SDK
# Download from: https://dotnet.microsoft.com/download/dotnet/9.0
```

#### 2. Port Already in Use
```bash
# Kill process using port 5000
sudo lsof -ti:5000 | xargs kill -9

# Or use different ports
dotnet run --urls "http://localhost:5001;https://localhost:5002"
```

#### 3. Docker Build Fails
```bash
# Clean Docker cache
docker system prune -a

# Rebuild without cache
docker build --no-cache -f Dockerfile.render -t jackson-ideas-web .
```

#### 4. Static Files Not Loading
- Check if `wwwroot` directory exists
- Verify files are in the correct location
- Check file permissions

### Debug Commands

```bash
# Check what files are in wwwroot
ls -la src/Jackson.Ideas.Web/wwwroot/

# Check if application is running
curl -s http://localhost:5000/health

# Check Docker container logs
docker logs jackson-ideas-test

# Check what's in the Docker image
docker run -it jackson-ideas-web /bin/bash
```

## Development Workflow

### 1. Local Development
1. Run `./local-dev.sh run`
2. Make changes to code
3. Application auto-reloads (hot reload)
4. Test changes in browser

### 2. Docker Testing
1. Build Docker image: `./local-dev.sh build-docker`
2. Run container: `./local-dev.sh run-docker`
3. Test all functionality
4. If issues found, fix and repeat

### 3. Production Deployment
1. Only deploy after local Docker testing passes
2. All static files should work locally first
3. All routes should be accessible

## Environment Variables

### Local Development
```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Data Source=jackson_ideas_dev.db
UseMockAuthentication=true
DemoMode__Enabled=true
DemoMode__UseRealAI=false
```

### Docker/Production
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=/app/data/jackson_ideas.db
UseMockAuthentication=true
DemoMode__Enabled=true
DemoMode__UseRealAI=false
```

## File Structure

```
Jackson.Ideas/
├── src/
│   └── Jackson.Ideas.Web/
│       ├── wwwroot/           # Static files
│       │   ├── app.css
│       │   ├── favicon.png
│       │   └── bootstrap/
│       ├── Components/
│       ├── Pages/
│       └── Program.cs
├── Dockerfile.render          # Production Docker configuration
├── local-dev.sh              # Local development script
└── LOCAL_DEVELOPMENT.md       # This file
```

## Success Criteria

Before deploying to Render, ensure:
- ✅ Application runs locally with `dotnet run`
- ✅ Static files load correctly (CSS, favicon, etc.)
- ✅ Main page loads without 404 errors
- ✅ Docker container builds successfully
- ✅ Docker container serves all files correctly
- ✅ Health endpoint returns healthy status

## Next Steps

Once local testing passes completely:
1. Commit changes to git
2. Push to GitHub
3. Render will auto-deploy
4. Monitor deployment with confidence

## Support

If you encounter issues:
1. Check the troubleshooting section
2. Verify all prerequisites are installed
3. Run the full test workflow
4. Check Docker logs for errors