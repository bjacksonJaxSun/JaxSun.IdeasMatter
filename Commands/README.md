# GitHub Commands for Product Development Workflow

This directory contains GitHub Actions workflows and supporting scripts for managing product development workflows entirely within GitHub (no Azure DevOps dependencies).

## Quick Start - Testing Vision Creation

### 1. Setup
First, ensure this `Commands` folder is in a GitHub repository with Actions enabled.

### 2. Create a Vision Using GitHub Actions

There are three ways to create a vision:

#### Option A: Using an existing vision file
```bash
# Navigate to Actions tab in GitHub
# Select "Create Product Vision" workflow
# Click "Run workflow"
# Fill in:
#   - Product name: "HR Portal"
#   - Vision file path: "Commands/docs/visions/example-hr-portal-vision.md"
#   - Preview: false (or true to test without creating anything)
```

#### Option B: Paste vision content directly
```bash
# In the workflow dispatch form, instead of file path:
#   - Product name: "Your Product"
#   - Vision content: [paste your vision markdown]
#   - Preview: false
```

#### Option C: Create vision via GitHub Issue
```bash
# Go to Issues > New Issue
# Select "Product Vision" template
# Fill out all sections
# Submit issue
```

### 3. Test the Vision Processing Script Locally

```bash
# Make script executable (if not already)
chmod +x Commands/scripts/github-cli/process-vision.sh

# Validate a vision document
./Commands/scripts/github-cli/process-vision.sh Commands/docs/visions/example-hr-portal-vision.md validate

# Generate a summary
./Commands/scripts/github-cli/process-vision.sh Commands/docs/visions/example-hr-portal-vision.md summary

# Extract metadata
./Commands/scripts/github-cli/process-vision.sh Commands/docs/visions/example-hr-portal-vision.md metadata
```

## What Gets Created

When you run the vision creation workflow, it will:

1. **Store Vision Document**: Save the vision to `/Commands/docs/visions/[product-name]/vision.md`
2. **Create GitHub Issue**: Create an issue labeled "vision" with a summary
3. **Create GitHub Project**: Set up a project board for tracking development
4. **Create Labels**: Ensure all required labels exist (vision, strategy, epic, etc.)
5. **Pin Issue**: Pin the vision issue to the repository for easy access

## Workflow Structure

```
Commands/
├── .github/
│   ├── workflows/
│   │   ├── create-vision.yml      # Create product vision
│   │   ├── create-strategy.yml    # Generate strategy (coming soon)
│   │   └── ...                    # More workflows to come
│   └── ISSUE_TEMPLATE/
│       ├── vision.yml             # Vision issue template
│       └── ...                    # More templates to come
├── scripts/
│   └── github-cli/
│       └── process-vision.sh      # Vision processing utilities
├── docs/
│   ├── visions/                   # Vision documents
│   └── strategies/                # Strategy documents (coming soon)
└── README.md                      # This file
```

## Complete Workflow

The full workflow from Vision to Implementation:

### 1. Create Vision
```bash
gh workflow run create-vision.yml \
  -f product_name='Ideas Matter' \
  -f vision_file_path='Commands/docs/visions/ideas-matter/vision.md' \
  -f preview=false
```
Creates: Vision Issue (#123), Project Board, Labels

### 2. Create Strategy
```bash
gh workflow run create-strategy.yml \
  -f vision_issue_number='123' \
  -f timeframe_months='18' \
  -f preview=false
```
Creates: Strategy Issue (#124), Milestone, Links to Vision

### 3. Create Epics
```bash
gh workflow run create-epics.yml \
  -f strategy_issue_number='124' \
  -f max_epics='5' \
  -f preview=false
```
Creates: 5 Epic Issues, Links to Strategy, Updates Project

### Workflow Hierarchy
```
Vision Issue (#123)
  └── Strategy Issue (#124)
      ├── Epic: Core Platform Development (#125)
      ├── Epic: User Interface and Experience (#126)
      ├── Epic: AI/ML Integration (#127)
      ├── Epic: Security and Compliance (#128)
      └── Epic: Integration and APIs (#129)
          └── Features (coming soon)
              └── User Stories (coming soon)
                  └── Tasks (coming soon)
```

## Preview Mode

All workflows support a preview mode that shows what would be created without actually creating anything. This is useful for testing and validation.

## Requirements

- GitHub repository with Actions enabled
- Appropriate permissions (write access to issues, projects, and repository)
- GitHub CLI (`gh`) is installed automatically in workflows

## Troubleshooting

### Vision validation fails
- Ensure your vision document includes all required sections
- Run the validation script locally to see specific errors

### Workflow fails with permissions error
- Check that Actions have write permissions in repository settings
- Ensure the workflow has the correct permissions block

### Can't find created resources
- Check the Actions run summary for links to created issues and projects
- Look for pinned issues in the repository