#!/bin/bash

# Script to convert DOCX to markdown and create vision
set -e

# Check if pandoc is installed
if ! command -v pandoc &> /dev/null; then
    echo "Installing pandoc..."
    sudo apt-get update
    sudo apt-get install -y pandoc
fi

# Input and output paths
DOCX_PATH="$1"
OUTPUT_DIR="Commands/docs/visions/ideas-matter"
OUTPUT_FILE="$OUTPUT_DIR/vision.md"

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Convert DOCX to Markdown
echo "Converting Word document to Markdown..."
pandoc -f docx -t markdown --wrap=preserve "$DOCX_PATH" -o "$OUTPUT_FILE"

# Clean up the markdown (remove Word-specific artifacts)
echo "Cleaning up markdown..."
sed -i 's/\[]{#[^}]*}//g' "$OUTPUT_FILE"  # Remove Word bookmarks
sed -i 's/{.underline}//g' "$OUTPUT_FILE"  # Remove underline markers
sed -i '/^$/N;/^\n$/d' "$OUTPUT_FILE"      # Remove multiple blank lines

echo "Vision document created at: $OUTPUT_FILE"
echo ""
echo "Preview of the converted vision:"
echo "================================"
head -n 50 "$OUTPUT_FILE"
echo "================================"
echo ""
echo "To create the vision in GitHub, run:"
echo "gh workflow run create-vision.yml -f product_name='Ideas Matter' -f vision_file_path='$OUTPUT_FILE'"