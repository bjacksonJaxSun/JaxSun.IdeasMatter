#!/usr/bin/env python3
"""
Convert DOCX Vision Statement to Markdown format
"""

import os
import sys
import re
from pathlib import Path

try:
    from docx import Document
except ImportError:
    print("Installing python-docx...")
    import subprocess
    subprocess.check_call([sys.executable, "-m", "pip", "install", "python-docx"])
    from docx import Document

def clean_text(text):
    """Clean up text from Word artifacts"""
    # Remove multiple spaces
    text = re.sub(r'\s+', ' ', text)
    # Remove leading/trailing whitespace
    text = text.strip()
    return text

def convert_docx_to_markdown(docx_path, output_path):
    """Convert DOCX to Markdown format"""
    doc = Document(docx_path)
    markdown_lines = []
    
    # Track current list level
    in_list = False
    list_level = 0
    
    for paragraph in doc.paragraphs:
        text = clean_text(paragraph.text)
        if not text:
            markdown_lines.append("")
            continue
        
        # Check paragraph style
        style_name = paragraph.style.name.lower() if paragraph.style else ""
        
        # Convert headings
        if 'heading 1' in style_name:
            markdown_lines.append(f"# {text}")
        elif 'heading 2' in style_name:
            markdown_lines.append(f"## {text}")
        elif 'heading 3' in style_name:
            markdown_lines.append(f"### {text}")
        elif 'heading 4' in style_name:
            markdown_lines.append(f"#### {text}")
        
        # Convert lists
        elif 'list' in style_name or paragraph.style.name.startswith('List'):
            # Determine list level and type
            if 'bullet' in style_name.lower() or text.startswith('•') or text.startswith('-'):
                # Bullet list
                text = text.lstrip('•-').strip()
                markdown_lines.append(f"- {text}")
            else:
                # Numbered list
                text = re.sub(r'^\d+\.?\s*', '', text)
                markdown_lines.append(f"1. {text}")
            in_list = True
        
        # Handle bold text
        elif paragraph.runs and any(run.bold for run in paragraph.runs):
            # This might be a section header without proper styling
            if len(text) < 50 and text.isupper():
                markdown_lines.append(f"## {text.title()}")
            else:
                markdown_lines.append(f"**{text}**")
        
        # Regular paragraph
        else:
            if in_list and not text.startswith(' '):
                in_list = False
            markdown_lines.append(text)
    
    # Join lines and clean up
    markdown_content = '\n\n'.join(line if line else '' for line in markdown_lines)
    
    # Additional formatting fixes
    markdown_content = re.sub(r'\n{3,}', '\n\n', markdown_content)  # Max 2 newlines
    
    # Write to file
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(markdown_content)
    
    return markdown_content

def main():
    if len(sys.argv) < 2:
        print("Usage: python convert_docx_to_vision.py <path_to_docx>")
        sys.exit(1)
    
    docx_path = sys.argv[1]
    if not os.path.exists(docx_path):
        print(f"Error: File not found: {docx_path}")
        sys.exit(1)
    
    # Output path
    output_dir = "Commands/docs/visions/ideas-matter"
    output_file = os.path.join(output_dir, "vision.md")
    
    print(f"Converting {docx_path} to Markdown...")
    content = convert_docx_to_markdown(docx_path, output_file)
    
    print(f"\nVision document created at: {output_file}")
    print("\nPreview of the converted vision:")
    print("=" * 50)
    print(content[:1000] + "..." if len(content) > 1000 else content)
    print("=" * 50)
    
    print("\nTo create the vision in GitHub, run:")
    print(f"gh workflow run create-vision.yml -f product_name='Ideas Matter' -f vision_file_path='{output_file}'")

if __name__ == "__main__":
    main()