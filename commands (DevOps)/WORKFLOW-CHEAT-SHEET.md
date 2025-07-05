# Azure DevOps Workflow Command Cheat Sheet

## Quick Start - Full Workflow with Approvals

### Step 1: Create Product Vision
```bash
./commands/vision/create-product-vision.ps1 -VisionName "Your Product Name"
# At prompt: Type Y and press Enter
```

### Step 2: Create Vision Strategy
```bash
./commands/strategy/build-vision-strategy-enhanced.ps1 -VisionEpicId [VISION_ID]
# At prompt: Type Y and press Enter
```

### Step 3: Create Epics
```bash
./commands/epics/build-epics.ps1 -StrategyEpicId [STRATEGY_ID] -MaxEpics 3
# At prompt: Type Y and press Enter
```

### Step 4: Create Features (for each Epic)
```bash
./commands/features/build-features.ps1 -EpicId [EPIC_ID] -MaxFeatures 5
# At prompt: Type Y and press Enter
```

### Step 5: Create Stories (for each Feature)
```bash
./commands/stories/build-stories.ps1 -FeatureId [FEATURE_ID] -MaxStories 3
# At prompt: Type Y and press Enter
```

### Step 6: Create Test Cases & Tasks (for each Story)
```bash
# Test Cases with proper steps
./commands/test-cases/build-test-cases.ps1 -StoryId [STORY_ID] -MaxTestCases 6
# At prompt: Type Y and press Enter

# Tasks
./commands/tasks/build-tasks.ps1 -StoryId [STORY_ID] -MaxTasks 6
# At prompt: Type Y and press Enter
```

---

## Approval Response Options

| Prompt | Response | Action |
|--------|----------|--------|
| `[Y] Yes [N] No` | **Y** | Proceed with creation |
| `[Y] Yes [N] No` | **N** | Cancel operation |
| `[Y] Yes [N] No [E] Edit` | **E** | Edit template before creation |
| `[Y] Yes [N] No [V] View` | **V** | View full content before deciding |

---

## Complete Example Walkthrough

```bash
# 1. CREATE VISION
./commands/vision/create-product-vision.ps1 -VisionName "HR Portal"
# Response: Y
# Output: Created Epic ID: 10123

# 2. CREATE STRATEGY
./commands/strategy/build-vision-strategy-enhanced.ps1 -VisionEpicId 10123
# Response: Y
# Output: Created Epic ID: 10124

# 3. CREATE EPICS
./commands/epics/build-epics.ps1 -StrategyEpicId 10124 -MaxEpics 2
# Response: Y
# Output: Created Epic IDs: 10125, 10126

# 4. CREATE FEATURES FOR FIRST EPIC
./commands/features/build-features.ps1 -EpicId 10125 -MaxFeatures 3
# Response: Y
# Output: Created Feature IDs: 10127, 10128, 10129

# 5. CREATE STORIES FOR FIRST FEATURE
./commands/stories/build-stories.ps1 -FeatureId 10127 -MaxStories 2
# Response: Y
# Output: Created Story IDs: 10130, 10131

# 6. CREATE TEST CASES FOR FIRST STORY
./commands/test-cases/build-test-cases.ps1 -StoryId 10130 -MaxTestCases 4
# Response: Y
# Output: Created Test Case IDs: 10132, 10133, 10134, 10135

# 7. CREATE TASKS FOR FIRST STORY
./commands/tasks/build-tasks.ps1 -StoryId 10130 -MaxTasks 3
# Response: Y
# Output: Created Task IDs: 10136, 10137, 10138
```

---

## Preview Mode Commands (No Creation)

```bash
# Add -Preview to any command to see what would be created
./commands/vision/create-product-vision.ps1 -VisionName "Test" -Preview
./commands/epics/build-epics.ps1 -StrategyEpicId 10124 -Preview
./commands/features/build-features.ps1 -EpicId 10125 -Preview
```

---

## Skip All Approvals (Automation Mode)

```bash
# Add -AutoApprove to any command
./commands/vision/create-product-vision.ps1 -VisionName "Test" -AutoApprove
./commands/epics/build-epics.ps1 -StrategyEpicId 10124 -AutoApprove
```

---

## Full Automated Workflow

```bash
# One command to rule them all (with approvals at each step)
./commands/workflow/execute-full-workflow.ps1 -VisionName "My Product"

# With limits
./commands/workflow/execute-full-workflow.ps1 -VisionName "My Product" \
    -MaxEpics 2 -MaxFeatures 3 -MaxStories 2 -MaxTestCases 5 -MaxTasks 3
```

---

## Check Progress

```bash
# Check workflow status
./commands/workflow/workflow-status.ps1 -VisionEpicId 10123

# Check all workflows
./commands/workflow/workflow-status.ps1
```

---

## Tips for Approval Process

1. **Always Preview First**: Use `-Preview` to see what will be created
2. **Start Small**: Use `-Max*` parameters to limit items created
3. **Take Notes**: Keep track of created IDs for next steps
4. **Review in DevOps**: Check created items in Azure DevOps before proceeding
5. **Use Status Command**: Check progress with workflow-status.ps1

---

## Common Patterns

### Minimal Test Run
```bash
# Create minimal hierarchy for testing
Vision (1) → Strategy (1) → Epics (2) → Features (2 each) → Stories (2 each)
```

### Full Product Setup
```bash
# Create complete hierarchy
Vision (1) → Strategy (1) → Epics (5-10) → Features (5-10 each) → Stories (5-10 each) → Test Cases & Tasks
```

### Update Existing Test Cases
```bash
# If test cases exist without steps
./commands/test-cases/update-test-case-steps.ps1 -TestCaseIds 10103,10104,10105
# Response: Y
```