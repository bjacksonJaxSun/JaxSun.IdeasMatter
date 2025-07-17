#!/bin/bash

# Script to demonstrate the complete GitHub workflow for Ideas Matter

echo "=================================================="
echo "Ideas Matter - Complete GitHub Workflow Guide"
echo "=================================================="
echo ""
echo "This guide shows how to run the complete workflow from Vision to Epics."
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}STEP 1: Create Vision${NC}"
echo "========================"
echo "The vision document is already created at:"
echo "  Commands/docs/visions/ideas-matter/vision.md"
echo ""
echo "To create the vision in GitHub:"
echo ""
echo "  gh workflow run create-vision.yml \\"
echo "    -f product_name='Ideas Matter' \\"
echo "    -f vision_file_path='Commands/docs/visions/ideas-matter/vision.md' \\"
echo "    -f preview=false"
echo ""
echo -e "${YELLOW}After running, note the Vision Issue number (e.g., #123)${NC}"
echo ""

echo -e "${BLUE}STEP 2: Create Strategy${NC}"
echo "=========================="
echo "Once you have the Vision Issue number, create the strategy:"
echo ""
echo "  gh workflow run create-strategy.yml \\"
echo "    -f vision_issue_number='123' \\"  # Replace with actual number
echo "    -f timeframe_months='18' \\"
echo "    -f preview=false"
echo ""
echo -e "${YELLOW}After running, note the Strategy Issue number (e.g., #124)${NC}"
echo ""

echo -e "${BLUE}STEP 3: Create Epics${NC}"
echo "======================"
echo "Once you have the Strategy Issue number, create epics:"
echo ""
echo "  gh workflow run create-epics.yml \\"
echo "    -f strategy_issue_number='124' \\"  # Replace with actual number
echo "    -f max_epics='5' \\"
echo "    -f preview=false"
echo ""
echo -e "${YELLOW}This will create 5 epic issues linked to your strategy${NC}"
echo ""

echo -e "${GREEN}Preview Mode${NC}"
echo "=============="
echo "You can run any workflow with preview=true to see what would be created:"
echo ""
echo "  gh workflow run create-vision.yml \\"
echo "    -f product_name='Ideas Matter' \\"
echo "    -f vision_file_path='Commands/docs/visions/ideas-matter/vision.md' \\"
echo "    -f preview=true"
echo ""

echo -e "${GREEN}Using GitHub Web Interface${NC}"
echo "============================"
echo "Alternatively, use the GitHub Actions tab in your repository:"
echo "1. Go to Actions tab"
echo "2. Select the workflow you want to run"
echo "3. Click 'Run workflow'"
echo "4. Fill in the required inputs"
echo "5. Click 'Run workflow' button"
echo ""

echo -e "${GREEN}Checking Progress${NC}"
echo "=================="
echo "Monitor your workflow progress:"
echo "- Actions tab: See workflow runs and logs"
echo "- Issues tab: View created issues with labels"
echo "- Projects tab: See the project board"
echo "- Milestones: Track strategy timeline"
echo ""

echo -e "${BLUE}Complete Example Flow${NC}"
echo "======================="
echo "# 1. Create Vision (returns issue #100)"
echo "gh workflow run create-vision.yml -f product_name='Ideas Matter' -f vision_file_path='Commands/docs/visions/ideas-matter/vision.md' -f preview=false"
echo ""
echo "# 2. Create Strategy from Vision #100 (returns issue #101)"
echo "gh workflow run create-strategy.yml -f vision_issue_number='100' -f timeframe_months='18' -f preview=false"
echo ""
echo "# 3. Create Epics from Strategy #101"
echo "gh workflow run create-epics.yml -f strategy_issue_number='101' -f max_epics='5' -f preview=false"
echo ""
echo "=================================================="