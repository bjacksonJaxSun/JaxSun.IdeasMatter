# Prompt to Continue Building GitHub Workflow System

## Context
I'm building a GitHub-based product development workflow system for "Ideas Matter" - an AI-powered platform that converts ideas into deployable code. The system replaces Azure DevOps workflows with pure GitHub implementations.

## Current Status

### ✅ Completed:
1. **Vision Creation Workflow** (`create-vision.yml`)
   - Processes vision documents (Word or Markdown)
   - Creates GitHub Issue with "vision" label
   - Validates vision structure
   - Claude Command: `Commands/claude-commands/create-vision-from-doc.sh`

2. **Strategy Generation Workflow** (`create-strategy.yml`)
   - Reads vision from GitHub issue
   - Generates 18-month strategy
   - Creates strategy issue and milestone

3. **Epics Creation Workflow** (`create-epics.yml`)
   - Creates up to 10 epic issues from strategy
   - Links epics to strategy and vision

4. **Infrastructure**
   - All workflows in `.github/workflows/`
   - Issue templates in `.github/ISSUE_TEMPLATE/`
   - Helper scripts in `Commands/scripts/`
   - Claude commands in `Commands/claude-commands/`

### 📍 Current State:
- Vision document exists: `Commands/docs/visions/ideas-matter/vision.md`
- Workflows are deployed and working
- Labels are created before use
- Project creation removed due to permissions

## What Needs to Be Done

### High Priority:
1. **Create Features Workflow** (`create-features.yml`)
   - Read epic issue
   - Generate 5-10 features per epic
   - Link features to epic

2. **Create Stories Workflow** (`create-stories.yml`)
   - Read feature issue
   - Generate user stories
   - Link stories to feature

3. **Create Tasks Workflow** (`create-tasks.yml`)
   - Generate development tasks from stories
   - Create test cases

### Medium Priority:
4. **Full Orchestration Workflow**
   - Run complete flow: Vision → Strategy → Epics → Features → Stories
   - Progress tracking and reporting

5. **Progress Dashboard**
   - Workflow status checking
   - Issue hierarchy visualization

### Low Priority:
6. **Documentation**
   - Complete setup guide
   - Migration from Azure DevOps guide

## Key Information

**Repository**: https://github.com/bjackson071968/Jackson.Ideas

**File Structure**:
```
.github/
├── workflows/
│   ├── create-vision.yml      ✓
│   ├── create-strategy.yml    ✓
│   ├── create-epics.yml       ✓
│   ├── create-features.yml    TODO
│   ├── create-stories.yml     TODO
│   └── create-tasks.yml       TODO
└── ISSUE_TEMPLATE/
    ├── vision.yml             ✓
    └── strategy.yml           ✓

Commands/
├── claude-commands/
│   ├── create-vision-from-doc.sh  ✓
│   └── run-gh-workflow.sh         ✓
├── docs/visions/ideas-matter/
│   └── vision.md                  ✓
└── scripts/
    └── github-cli/
        └── process-vision.sh      ✓
```

**Labels Created**:
- vision (blue)
- strategy (blue) 
- epic (purple)
- feature (green)
- story (gold)
- task (orange)
- test (red)

## Continuation Instructions

Please continue building the GitHub workflow system by:
1. Creating the features workflow that reads epics and generates features
2. Following the same pattern as existing workflows
3. Maintaining consistency with error handling and permissions
4. Creating corresponding Claude commands for automation

The goal is to have a complete workflow from Vision → Strategy → Epics → Features → Stories → Tasks, all automated through GitHub Actions.

## Technical Notes
- No `projects: write` permission (use only: contents, issues, pull-requests)
- Use `gh` CLI for GitHub operations
- Python available for complex logic
- All workflows support preview mode
- Vision for "Ideas Matter" is ready to process