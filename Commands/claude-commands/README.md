# Claude Code Commands

This directory contains special commands designed for Claude Code to automate GitHub workflows.

## Available Commands

### create-vision-from-doc.sh

Automates the entire process of creating a product vision in GitHub from a document.

**Features:**
- Accepts Word (.docx) or Markdown (.md) documents
- Converts and validates vision structure
- Commits to Git and pushes to GitHub
- Triggers GitHub workflow automatically
- Provides preview mode for testing

**Usage:**
```bash
# Create vision from Word document
./Commands/claude-commands/create-vision-from-doc.sh "/path/to/vision.docx" "Product Name"

# Create vision from Markdown document
./Commands/claude-commands/create-vision-from-doc.sh "/path/to/vision.md" "Product Name"

# Preview mode (no changes made)
./Commands/claude-commands/create-vision-from-doc.sh "/path/to/vision.md" "Product Name" --preview
```

**What it does:**
1. **Process Document**: Converts DOCX to Markdown or processes existing Markdown
2. **Validate Structure**: Ensures all required vision sections are present
3. **Git Operations**: Commits and pushes the vision to GitHub
4. **Run Workflow**: Triggers the create-vision.yml GitHub Action

**Requirements:**
- Git repository with GitHub remote
- GitHub CLI (`gh`) installed and authenticated
- Python 3 for document processing
- Write permissions to the repository

## For Ideas Matter Vision

To create the Ideas Matter vision from the existing document:

```bash
# Using the Markdown version
./Commands/claude-commands/create-vision-from-doc.sh \
  "/mnt/c/Development/Jackson.Ideas/PMDocs/Vision Statement.md" \
  "Ideas Matter"

# Or if you still have the Word version
./Commands/claude-commands/create-vision-from-doc.sh \
  "/mnt/c/Development/Jackson.Ideas/PMDocs/Vision Statement.docx" \
  "Ideas Matter"
```

## How Claude Code Should Use These Commands

When asked to create a vision in GitHub:

1. **Check if document exists**:
   ```bash
   ls -la "/path/to/vision/document"
   ```

2. **Run the command**:
   ```bash
   ./Commands/claude-commands/create-vision-from-doc.sh "/path/to/doc" "Product Name"
   ```

3. **Monitor the output** for success or error messages

4. **Check the results**:
   - Vision file created at: `Commands/docs/visions/[product-name]/vision.md`
   - Git commit and push status
   - GitHub workflow trigger status

## Preview Mode

Always test with `--preview` first:
```bash
./Commands/claude-commands/create-vision-from-doc.sh \
  "/path/to/vision.md" \
  "Product Name" \
  --preview
```

This will:
- Process the document
- Show what would be created
- Skip Git operations
- Skip workflow execution

## Troubleshooting

### GitHub CLI not installed
The command will provide instructions to install `gh` from https://cli.github.com/

### Git push fails
- Check GitHub authentication
- Ensure you have push permissions
- Verify remote is configured

### Document conversion fails
- Ensure Python 3 is installed
- Check document is readable
- Verify document contains vision content

### Validation fails
- Check that document includes required sections
- Run validation manually: `./Commands/scripts/github-cli/process-vision.sh <vision.md> validate`

## Next Steps

After vision creation completes:
1. Check GitHub Issues for the created vision
2. Note the issue number
3. Run strategy generation with that issue number
4. Continue with epics, features, and stories