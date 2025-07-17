# Ideas Matter Vision

This directory contains the product vision for Ideas Matter - an AI-powered platform that transforms ideas into deployable code.

## Vision Document

The complete vision is in `vision.md` and includes:
- Vision Statement
- Target Market Analysis
- Key Problems Addressed
- Strategic Themes
- Success Metrics
- Key Differentiators
- Constraints and Assumptions
- High-Level Roadmap
- Risk Analysis

## Creating the Vision in GitHub

### Prerequisites
1. Push the Commands folder to your GitHub repository
2. Ensure GitHub Actions are enabled in your repository
3. Have appropriate permissions (write access to issues, projects, and repository)

### Steps to Create Vision

#### Option 1: Using GitHub Web Interface (Recommended)
1. Navigate to your repository on GitHub
2. Click on the **Actions** tab
3. Find and select **"Create Product Vision"** workflow
4. Click **"Run workflow"**
5. Fill in the form:
   - **Product name**: `Ideas Matter`
   - **Vision file path**: `Commands/docs/visions/ideas-matter/vision.md`
   - **Preview**: `false` (or `true` to test without creating anything)
6. Click **"Run workflow"** button

#### Option 2: Using GitHub CLI
```bash
# Install GitHub CLI if not already installed
# https://cli.github.com/

# Run the workflow
gh workflow run create-vision.yml \
  -f product_name='Ideas Matter' \
  -f vision_file_path='Commands/docs/visions/ideas-matter/vision.md' \
  -f preview=false
```

### What Gets Created

When the workflow runs successfully, it will:

1. **GitHub Issue**: Creates an issue titled "Vision: Ideas Matter" with:
   - Vision summary
   - Link to full vision document
   - Next steps for strategy creation
   - Pinned to repository for visibility

2. **GitHub Project**: Creates "Ideas Matter Development" project board for tracking

3. **Labels**: Creates workflow labels if they don't exist:
   - `vision` (blue)
   - `strategy` (blue)
   - `epic` (purple)
   - `feature` (green)
   - `story` (gold)
   - `task` (orange)
   - `test` (red)

4. **Repository Files**: Commits the vision document to the repository

### Verification

After running the workflow:
1. Check the Actions tab for the workflow run status
2. Look for the new pinned issue in your repository
3. Navigate to Projects tab to see the new project board
4. Verify the vision document is committed

### Next Steps

Once the vision is created:
1. Review the created issue and project
2. Run the Strategy Generation workflow (coming soon)
3. Begin breaking down the vision into epics and features

### Troubleshooting

**Workflow not visible**: Ensure the Commands folder is pushed to the default branch

**Permission errors**: Check repository settings → Actions → General → Workflow permissions

**File not found**: Verify the exact path: `Commands/docs/visions/ideas-matter/vision.md`

**Preview mode**: Always test with `preview=true` first to see what will be created