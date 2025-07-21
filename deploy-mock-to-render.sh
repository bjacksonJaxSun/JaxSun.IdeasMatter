#!/bin/bash

# Deploy Jackson Ideas Mock Application to Render
echo "🚀 Deploying Jackson Ideas Mock Application to Render..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Export GitHub CLI path
export PATH="$HOME/.local/bin:$PATH"

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if we're in the right directory
if [ ! -f "render-mock.yaml" ]; then
    print_error "render-mock.yaml not found. Please run this script from the project root."
    exit 1
fi

# Check if we have the Mock project
if [ ! -d "src/Jackson.Ideas.Mock" ]; then
    print_error "Mock project not found at src/Jackson.Ideas.Mock"
    exit 1
fi

# Check if we're authenticated with GitHub CLI
print_status "Checking GitHub CLI authentication..."
if ! gh auth status > /dev/null 2>&1; then
    print_error "GitHub CLI not authenticated. Please run: gh auth login"
    exit 1
fi

print_success "GitHub CLI authentication verified"

# Validate project files instead of building (Docker will handle compilation)
print_status "Validating Mock project files..."
if [ ! -f "src/Jackson.Ideas.Mock/Jackson.Ideas.Mock.csproj" ]; then
    print_error "Mock project file not found. Please ensure the Mock project exists."
    exit 1
fi

if [ ! -f "src/Jackson.Ideas.Mock/Program.cs" ]; then
    print_error "Mock Program.cs not found. Please ensure the Mock project is complete."
    exit 1
fi

print_success "Mock project files validated (Docker will handle compilation)"

# Check for uncommitted changes
print_status "Checking for uncommitted changes..."
if ! git diff --quiet || ! git diff --cached --quiet; then
    print_warning "Found uncommitted changes. Committing them now..."
    git add .
    
    # Create a commit message for Mock deployment
    git commit -m "Prepare Mock application for Render deployment

🔧 Changes Made:
- Added render-mock.yaml configuration for Mock project deployment
- Created Dockerfile.mock optimized for Mock application
- Added deployment script for automated Mock deployment
- Configured environment variables for demo/mock mode

🎯 Mock Application Features:
- Standalone Blazor Server application
- Pre-loaded demo scenarios and data
- Simulated processing delays for realistic demo experience
- No external dependencies or API requirements

🚀 Deployment Ready:
- Docker containerized for Render deployment
- Environment configured for production demo mode
- Health checks and monitoring enabled
- Auto-deploy from main branch configured

🤖 Generated with [Claude Code](https://claude.ai/code)

Co-Authored-By: Claude <noreply@anthropic.com>"

    if [ $? -ne 0 ]; then
        print_error "Failed to commit changes"
        exit 1
    fi
    
    print_success "Changes committed successfully"
else
    print_status "No uncommitted changes found"
fi

# Push to GitHub
print_status "Pushing changes to GitHub..."
if ! git push origin main; then
    print_error "Failed to push to GitHub"
    exit 1
fi

print_success "Successfully pushed to GitHub"

# Display deployment information
echo ""
echo "📋 Mock Application Deployment Information:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🏗️  Configuration File: render-mock.yaml"
echo "🐳  Docker File: Dockerfile.mock"
echo "📁  Project Path: src/Jackson.Ideas.Mock"
echo "🌐  Runtime: .NET 9 Blazor Server"
echo "🎭  Mode: Demo/Mock with simulated data"
echo ""
echo "🔗 Next Steps for Render Setup:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "1. 🌐 Go to https://dashboard.render.com"
echo "2. ➕ Click 'New +' → 'Blueprint'"
echo "3. 🔗 Connect your GitHub repository"
echo "4. 📄 Select 'render-mock.yaml' as the blueprint file"
echo "5. ⚙️  Review and confirm the configuration"
echo "6. 🚀 Click 'Apply' to start deployment"
echo ""
echo "📊 Expected Configuration:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "• Service Name: jackson-ideas-mock"
echo "• Region: Oregon (US West)"
echo "• Plan: Free (750 hours/month)"
echo "• Auto-deploy: Enabled from main branch"
echo "• Health Check: Enabled at root path"
echo ""
echo "🎯 Mock Application Features:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "• 📊 Pre-loaded business idea scenarios"
echo "• 💰 Sample financial projections"
echo "• 🔍 Mock market research data"
echo "• 👥 Demo user profiles and sessions"
echo "• ⏱️  Realistic processing delays simulation"
echo "• 🎨 Complete UI/UX demonstration"
echo ""
echo "💡 Manual Render Setup Alternative:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "If blueprint deployment doesn't work:"
echo "1. Create new Web Service manually"
echo "2. Connect GitHub repository"
echo "3. Set Runtime: Docker"
echo "4. Set Dockerfile Path: ./Dockerfile.mock"
echo "5. Configure environment variables from render-mock.yaml"
echo ""

print_success "Mock application deployment preparation completed!"
print_status "Repository updated and ready for Render deployment via Blueprint or manual setup."

echo ""
echo "✨ Your Jackson Ideas Mock application is ready to deploy to Render!"