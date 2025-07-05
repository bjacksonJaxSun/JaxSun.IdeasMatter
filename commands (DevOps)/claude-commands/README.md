# Claude Commands for Azure DevOps Workflow

These wrapper scripts allow Claude to execute the workflow commands with automatic approval responses, eliminating the need for manual interaction during the approval process.

## Available Claude Commands

### Individual Step Commands

Each command automatically responds with "Y" to approval prompts:

```powershell
# 1. Create Product Vision
./commands/claude-commands/create-vision-approved.ps1 -VisionName "Your Product"

# 2. Create Vision Strategy
./commands/claude-commands/create-strategy-approved.ps1 -VisionEpicId 10123

# 3. Create Epics
./commands/claude-commands/create-epics-approved.ps1 -StrategyEpicId 10124 -MaxEpics 3

# 4. Create Features
./commands/claude-commands/create-features-approved.ps1 -EpicId 10125 -MaxFeatures 5

# 5. Create User Stories
./commands/claude-commands/create-stories-approved.ps1 -FeatureId 10127 -MaxStories 3

# 6. Create Test Cases
./commands/claude-commands/create-test-cases-approved.ps1 -StoryId 10130 -MaxTestCases 6

# 7. Create Tasks
./commands/claude-commands/create-tasks-approved.ps1 -StoryId 10130 -MaxTasks 6
```

### Full Workflow Command

Execute the entire workflow with automatic approvals:

```powershell
./commands/claude-commands/execute-full-workflow-approved.ps1 -VisionName "New Product" -MaxEpics 2 -MaxFeatures 3 -MaxStories 2
```

## How It Works

These wrapper scripts:
1. Accept the same parameters as the original commands
2. Automatically pipe "Y" responses to approval prompts
3. Show progress messages for Claude and users
4. Support `-Preview` mode for dry runs

## Example Claude Session

When Claude needs to create a workflow, it can use these commands:

```
User: "Create a product vision for an Employee Portal"
Claude: I'll create that for you using the approved command:

<uses bash tool>
./commands/claude-commands/create-vision-approved.ps1 -VisionName "Employee Portal"
</uses>

The vision was created with ID 10123. Now I'll create the strategy:

<uses bash tool>
./commands/claude-commands/create-strategy-approved.ps1 -VisionEpicId 10123
</uses>
```

## Preview Mode

All commands support `-Preview` to see what would be created without actually creating items:

```powershell
./commands/claude-commands/create-epics-approved.ps1 -StrategyEpicId 10124 -MaxEpics 3 -Preview
```

## Benefits

1. **No Manual Intervention**: Claude can execute complete workflows without waiting for approval responses
2. **Same Parameters**: All original command parameters are supported
3. **Traceable**: Shows clear messages about what's being auto-approved
4. **Safe Preview**: Preview mode works without approvals
5. **Consistent**: Maintains the same workflow structure as manual execution

## Notes

- These commands are specifically designed for Claude automation
- For manual execution with approval prompts, use the original commands in parent directories
- The scripts show "Auto-approving creation..." messages for transparency
- All created items are tracked and IDs are displayed in the output