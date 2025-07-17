#!/bin/bash

# Script to process vision documents and extract key information
# Used by GitHub Actions workflows

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to extract section content from markdown
extract_section() {
    local file="$1"
    local section="$2"
    local level="${3:-2}" # Default to h2 (##)
    
    # Create the header pattern based on level
    local header_pattern=""
    for ((i=1; i<=level; i++)); do
        header_pattern+="#"
    done
    header_pattern+=" $section"
    
    # Extract content between this section and the next section of same or higher level
    awk -v section="$header_pattern" -v level="$level" '
        BEGIN { found = 0; header_regex = "^#{1," level "} " }
        $0 ~ section { found = 1; next }
        found && $0 ~ header_regex { exit }
        found { print }
    ' "$file"
}

# Function to validate vision document structure
validate_vision() {
    local file="$1"
    local errors=0
    
    echo "Validating vision document structure..."
    
    # Check for required sections
    local required_sections=(
        "Vision Statement"
        "Target Market"
        "Key Problems"
        "Strategic Themes"
        "Success Metrics"
        "Key Differentiators"
    )
    
    for section in "${required_sections[@]}"; do
        if ! grep -q "## $section" "$file" && ! grep -q "### $section" "$file"; then
            echo -e "${RED}✗ Missing required section: $section${NC}"
            ((errors++))
        else
            echo -e "${GREEN}✓ Found section: $section${NC}"
        fi
    done
    
    if [ $errors -gt 0 ]; then
        echo -e "${RED}Vision validation failed with $errors errors${NC}"
        return 1
    else
        echo -e "${GREEN}Vision validation passed!${NC}"
        return 0
    fi
}

# Function to generate vision summary
generate_summary() {
    local file="$1"
    local max_lines="${2:-10}"
    
    echo "# Vision Summary"
    echo ""
    
    # Extract vision statement
    local vision_statement=$(extract_section "$file" "Vision Statement")
    if [ -n "$vision_statement" ]; then
        echo "## Vision Statement"
        echo "$vision_statement" | head -n 3
        echo ""
    fi
    
    # Extract strategic themes
    local themes=$(extract_section "$file" "Strategic Themes")
    if [ -n "$themes" ]; then
        echo "## Strategic Themes"
        echo "$themes" | grep -E "^\s*[-*•]|^[0-9]+\." | head -n 5
        echo ""
    fi
    
    # Extract key problems
    local problems=$(extract_section "$file" "Key Problems")
    if [ -n "$problems" ]; then
        echo "## Key Problems"
        echo "$problems" | grep -E "^\s*[-*•]|^[0-9]+\." | head -n 3
    fi
}

# Function to extract metadata for issue creation
extract_metadata() {
    local file="$1"
    
    # Extract product name from title or first heading
    local product_name=$(grep -m1 "^# " "$file" | sed 's/^# //' | sed 's/Product Vision[: -]*//')
    
    # Extract timeframe if mentioned
    local timeframe=$(grep -i "timeframe\|timeline\|duration" "$file" | head -n1 | grep -oE "[0-9]+ (months?|years?)" | head -n1)
    
    # Count strategic themes
    local theme_count=$(extract_section "$file" "Strategic Themes" | grep -cE "^\s*[-*•]|^[0-9]+\." || echo "0")
    
    # Output as JSON
    cat <<EOF
{
    "product_name": "$product_name",
    "timeframe": "$timeframe",
    "theme_count": $theme_count
}
EOF
}

# Main processing logic
main() {
    local vision_file="$1"
    local command="${2:-validate}"
    
    if [ ! -f "$vision_file" ]; then
        echo -e "${RED}Error: Vision file not found: $vision_file${NC}"
        exit 1
    fi
    
    case "$command" in
        validate)
            validate_vision "$vision_file"
            ;;
        summary)
            generate_summary "$vision_file"
            ;;
        metadata)
            extract_metadata "$vision_file"
            ;;
        *)
            echo "Usage: $0 <vision_file> [validate|summary|metadata]"
            exit 1
            ;;
    esac
}

# Run main function if script is executed directly
if [ "${BASH_SOURCE[0]}" == "${0}" ]; then
    main "$@"
fi