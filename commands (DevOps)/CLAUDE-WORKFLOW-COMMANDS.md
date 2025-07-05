# Claude Workflow Commands - Cheat Sheet

## 🤖 Claude-Executable Commands (Auto-Approved)

### Complete Workflow in One Command
```bash
./commands/claude-commands/execute-full-workflow-approved.ps1 -VisionName "Product Name" -MaxEpics 2 -MaxFeatures 3 -MaxStories 2
```

### Step-by-Step Commands for Claude

```bash
# 1. CREATE VISION
./commands/claude-commands/create-vision-approved.ps1 -VisionName "Your Product"
# Output: Created Epic ID: 10123

# 2. CREATE STRATEGY (use Vision ID)
./commands/claude-commands/create-strategy-approved.ps1 -VisionEpicId 10123
# Output: Created Epic ID: 10124

# 3. CREATE EPICS (use Strategy ID)
./commands/claude-commands/create-epics-approved.ps1 -StrategyEpicId 10124 -MaxEpics 3
# Output: Created Epic IDs: 10125, 10126, 10127

# 4. CREATE FEATURES (use Epic ID)
./commands/claude-commands/create-features-approved.ps1 -EpicId 10125 -MaxFeatures 3
# Output: Created Feature IDs: 10128, 10129, 10130

# 5. CREATE STORIES (use Feature ID)
./commands/claude-commands/create-stories-approved.ps1 -FeatureId 10128 -MaxStories 3
# Output: Created Story IDs: 10131, 10132, 10133

# 6. CREATE TEST CASES (use Story ID)
./commands/claude-commands/create-test-cases-approved.ps1 -StoryId 10131 -MaxTestCases 6
# Output: Created Test Case IDs: 10134-10139

# 7. CREATE TASKS (use Story ID)
./commands/claude-commands/create-tasks-approved.ps1 -StoryId 10131 -MaxTasks 6
# Output: Created Task IDs: 10140-10145
```

## 📊 Check Status
```bash
# Standard status command (no approval needed)
./commands/workflow/workflow-status.ps1 -VisionEpicId 10123
```

## 🔍 Preview Mode (No Creation)
```bash
# Add -Preview to any Claude command
./commands/claude-commands/create-vision-approved.ps1 -VisionName "Test" -Preview
./commands/claude-commands/create-epics-approved.ps1 -StrategyEpicId 10124 -Preview
```

## 💡 Claude Usage Pattern

When a user asks Claude to create a workflow:

```
User: "Create a complete workflow for Employee Portal with 2 epics, 3 features each"

Claude: I'll create the Employee Portal workflow for you:

<bash>
./commands/claude-commands/execute-full-workflow-approved.ps1 -VisionName "Employee Portal" -MaxEpics 2 -MaxFeatures 3 -MaxStories 3
</bash>

This will create:
- 1 Product Vision
- 1 Vision Strategy  
- 2 Epics
- 6 Features (3 per Epic)
- 18 Stories (3 per Feature)
- Test Cases and Tasks for each Story
```

## 🎯 Key Differences

| Regular Commands | Claude Commands |
|-----------------|-----------------|
| Prompts for approval | Auto-approves with "Y" |
| Requires manual input | Fully automated |
| Located in `/commands/*` | Located in `/commands/claude-commands/*` |
| For human interaction | For Claude automation |

## ⚡ Quick Test Run

```bash
# Minimal test hierarchy
./commands/claude-commands/execute-full-workflow-approved.ps1 \
  -VisionName "Test Product" \
  -MaxEpics 1 \
  -MaxFeatures 2 \
  -MaxStories 2 \
  -MaxTestCases 3 \
  -MaxTasks 3
```

## 📝 Notes

- All commands show "Auto-approving creation..." for transparency
- IDs are displayed in output for tracking
- Same parameters as original commands
- Safe to use in any Claude session
- Preview mode available for all commands