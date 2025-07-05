# Azure DevOps Command Scripts

This directory contains PowerShell scripts for managing the complete product development workflow in Azure DevOps.

## Directory Structure

```
commands/
├── common/
│   └── azure-devops-api.ps1        # Shared API functions for all scripts
├── vision/
│   └── create-product-vision.ps1   # Creates Product Vision epics
├── strategy/
│   └── build-vision-strategy-enhanced.ps1  # Creates Vision Strategy from Vision
├── epics/
│   └── build-epics.ps1            # Creates Epics from Vision Strategy
├── features/
│   └── build-features.ps1         # Creates Features from Epics
├── stories/
│   └── build-stories.ps1           # Creates User Stories from Features
├── test-cases/
│   ├── build-test-cases.ps1       # Creates Test Cases with steps from Stories
│   └── update-test-case-steps.ps1 # Updates existing Test Cases with steps
├── tasks/
│   └── build-tasks.ps1            # Creates Tasks from Stories
└── workflow/
    ├── execute-full-workflow.ps1  # Orchestrates the complete workflow
    └── workflow-status.ps1        # Shows workflow execution status
```

## Workflow Hierarchy

The scripts follow the Azure DevOps work item hierarchy:

```
Product Vision (Epic)
    └── Vision Strategy (Epic)
        └── Implementation Epics (1-20)
            └── Features (1-40 per Epic)
                └── User Stories (1-40 per Feature)
                    ├── Test Cases (1-40 per Story)
                    └── Tasks (1-40 per Story)
```

## Usage Examples

### Create a New Product Vision
```powershell
./commands/vision/create-product-vision.ps1 -VisionName "HR Platform" -Preview
./commands/vision/create-product-vision.ps1 -VisionName "HR Platform" -AutoApprove
```

### Build Complete Hierarchy from Vision
```powershell
# Create strategy from vision
./commands/strategy/build-vision-strategy-enhanced.ps1 -VisionEpicId 10090 -AutoApprove

# Create epics from strategy
./commands/epics/build-epics.ps1 -StrategyEpicId 10091 -MaxEpics 3 -AutoApprove

# Create features from epic
./commands/features/build-features.ps1 -EpicId 10092 -MaxFeatures 5 -AutoApprove

# Create stories from feature
./commands/stories/build-stories.ps1 -FeatureId 10095 -MaxStories 3 -AutoApprove

# Create test cases from story
./commands/test-cases/build-test-cases.ps1 -StoryId 10100 -MaxTestCases 6 -AutoApprove

# Create tasks from story
./commands/tasks/build-tasks.ps1 -StoryId 10100 -MaxTasks 6 -AutoApprove
```

### Update Existing Test Cases
```powershell
# Preview what will be updated
./commands/test-cases/update-test-case-steps.ps1 -TestCaseIds 10103,10104,10105 -Preview

# Apply updates
./commands/test-cases/update-test-case-steps.ps1 -TestCaseIds 10103,10104,10105
```

### Execute Full Workflow
```powershell
# Preview full workflow
./commands/workflow/execute-full-workflow.ps1 -VisionName "New Product" -Preview

# Execute with specific limits
./commands/workflow/execute-full-workflow.ps1 -VisionName "New Product" -MaxEpics 2 -MaxFeatures 3 -MaxStories 2
```

## Common Parameters

Most scripts support these common parameters:

- `-Preview`: Shows what would be created without making changes
- `-AutoApprove`: Skips confirmation prompts
- `-Max*`: Limits the number of items created (e.g., `-MaxEpics 5`)

## Test Case Steps

The test case scripts create proper Azure DevOps test steps with:
- **Action**: What to do
- **Expected Result**: What should happen
- Context-specific steps based on test type (Unit, Integration, E2E)

## Notes

- All scripts use the configuration in `/azuredevops.config.json`
- The `common/azure-devops-api.ps1` file provides shared functions for API calls
- Scripts are designed to be idempotent where possible
- Version control is managed through Git - no version numbers in filenames