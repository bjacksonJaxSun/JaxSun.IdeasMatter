#!/bin/bash

# Claude Code GitHub Workflow Runner
# This script attempts multiple methods to run GitHub workflows

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Function to print status
print_status() {
    echo -e "${BLUE}[$(date +'%H:%M:%S')]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[$(date +'%H:%M:%S')] ✓${NC} $1"
}

print_error() {
    echo -e "${RED}[$(date +'%H:%M:%S')] ✗${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[$(date +'%H:%M:%S')] !${NC} $1"
}

# Parse arguments
WORKFLOW_FILE="$1"
shift
WORKFLOW_ARGS="$@"

if [ -z "$WORKFLOW_FILE" ]; then
    print_error "Usage: $0 <workflow-file> [workflow arguments]"
    exit 1
fi

print_status "Attempting to run GitHub workflow: $WORKFLOW_FILE"

# Method 1: Try direct gh command
print_status "Method 1: Trying direct gh command..."
if command -v gh &> /dev/null; then
    print_success "GitHub CLI found in PATH"
    if gh workflow run "$WORKFLOW_FILE" $WORKFLOW_ARGS; then
        print_success "Workflow triggered successfully!"
        exit 0
    else
        print_warning "Direct gh command failed, trying next method..."
    fi
else
    print_warning "gh not found in PATH"
fi

# Method 2: Try Windows gh.exe from WSL
print_status "Method 2: Trying Windows gh.exe..."
if [ -f "/mnt/c/Program Files/GitHub CLI/gh.exe" ]; then
    print_status "Found gh.exe in Program Files"
    if "/mnt/c/Program Files/GitHub CLI/gh.exe" workflow run "$WORKFLOW_FILE" $WORKFLOW_ARGS; then
        print_success "Workflow triggered successfully via gh.exe!"
        exit 0
    else
        print_warning "gh.exe failed, trying next method..."
    fi
elif [ -f "/mnt/c/Program Files (x86)/GitHub CLI/gh.exe" ]; then
    print_status "Found gh.exe in Program Files (x86)"
    if "/mnt/c/Program Files (x86)/GitHub CLI/gh.exe" workflow run "$WORKFLOW_FILE" $WORKFLOW_ARGS; then
        print_success "Workflow triggered successfully via gh.exe!"
        exit 0
    else
        print_warning "gh.exe failed, trying next method..."
    fi
else
    print_warning "gh.exe not found in standard locations"
fi

# Method 3: Try PowerShell
print_status "Method 3: Trying PowerShell..."
if command -v powershell.exe &> /dev/null; then
    print_status "Running via PowerShell..."
    POWERSHELL_CMD="gh workflow run '$WORKFLOW_FILE' $WORKFLOW_ARGS"
    if powershell.exe -Command "$POWERSHELL_CMD"; then
        print_success "Workflow triggered successfully via PowerShell!"
        exit 0
    else
        print_warning "PowerShell method failed, trying next..."
    fi
else
    print_warning "PowerShell not available"
fi

# Method 4: Try cmd.exe
print_status "Method 4: Trying cmd.exe..."
if command -v cmd.exe &> /dev/null; then
    print_status "Running via cmd.exe..."
    CMD_COMMAND="gh workflow run $WORKFLOW_FILE $WORKFLOW_ARGS"
    if cmd.exe /c "$CMD_COMMAND"; then
        print_success "Workflow triggered successfully via cmd.exe!"
        exit 0
    else
        print_warning "cmd.exe method failed"
    fi
else
    print_warning "cmd.exe not available"
fi

# Method 5: Generate a script for manual execution
print_status "Method 5: Generating scripts for manual execution..."

# Create directory for generated scripts
SCRIPT_DIR="/mnt/c/Development/Jackson.Ideas/Commands/generated-scripts"
mkdir -p "$SCRIPT_DIR"

# Generate batch file
BAT_FILE="$SCRIPT_DIR/run-workflow.bat"
cat > "$BAT_FILE" << EOF
@echo off
cd /d C:\\Development\\Jackson.Ideas
gh workflow run $WORKFLOW_FILE $WORKFLOW_ARGS
pause
EOF
print_success "Created batch file: $BAT_FILE"

# Generate PowerShell file
PS1_FILE="$SCRIPT_DIR/run-workflow.ps1"
cat > "$PS1_FILE" << EOF
Set-Location "C:\\Development\\Jackson.Ideas"
gh workflow run '$WORKFLOW_FILE' $WORKFLOW_ARGS
Read-Host "Press Enter to continue"
EOF
print_success "Created PowerShell script: $PS1_FILE"

print_error "Could not run workflow automatically"
print_warning "Manual execution required. You have several options:"
echo ""
echo "Option 1: Run from Windows Command Prompt:"
echo "  cd C:\\Development\\Jackson.Ideas"
echo "  gh workflow run $WORKFLOW_FILE $WORKFLOW_ARGS"
echo ""
echo "Option 2: Double-click the generated script:"
echo "  $(wslpath -w "$BAT_FILE")"
echo ""
echo "Option 3: Use GitHub Web Interface:"
echo "  1. Go to https://github.com/[your-repo]/actions"
echo "  2. Select the workflow"
echo "  3. Click 'Run workflow'"
echo ""

# Try to open the batch file in Windows Explorer
if command -v explorer.exe &> /dev/null; then
    print_status "Opening script location in Windows Explorer..."
    explorer.exe "$(wslpath -w "$SCRIPT_DIR")"
fi

exit 1