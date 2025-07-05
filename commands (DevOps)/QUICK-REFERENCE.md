# Quick Reference Card - Azure DevOps Workflow

## 🚀 Rapid Workflow Execution

### Copy-Paste Commands (Replace IDs as you go)

```powershell
# 1. VISION
./commands/vision/create-product-vision.ps1 -VisionName "YOUR_PRODUCT"
# Type: Y

# 2. STRATEGY (use Vision ID from above)
./commands/strategy/build-vision-strategy-enhanced.ps1 -VisionEpicId YOUR_VISION_ID
# Type: Y

# 3. EPICS (use Strategy ID from above)
./commands/epics/build-epics.ps1 -StrategyEpicId YOUR_STRATEGY_ID -MaxEpics 3
# Type: Y

# 4. FEATURES (use Epic ID from above)
./commands/features/build-features.ps1 -EpicId YOUR_EPIC_ID -MaxFeatures 3
# Type: Y

# 5. STORIES (use Feature ID from above)
./commands/stories/build-stories.ps1 -FeatureId YOUR_FEATURE_ID -MaxStories 3
# Type: Y

# 6. TEST CASES (use Story ID from above)
./commands/test-cases/build-test-cases.ps1 -StoryId YOUR_STORY_ID -MaxTestCases 6
# Type: Y

# 7. TASKS (use same Story ID)
./commands/tasks/build-tasks.ps1 -StoryId YOUR_STORY_ID -MaxTasks 6
# Type: Y
```

## 📋 Approval Responses

| When you see | Type | Result |
|--------------|------|--------|
| `[Y] Yes [N] No` | **Y** | ✅ Create items |
| `[Y] Yes [N] No` | **N** | ❌ Cancel |
| `[E] Edit template` | **E** | ✏️ Edit first |
| `[V] View full` | **V** | 👁️ See details |

## 🎯 One-Liner Full Workflow

```powershell
# Complete workflow with all approvals
./commands/workflow/execute-full-workflow.ps1 -VisionName "My Product" -MaxEpics 2 -MaxFeatures 3 -MaxStories 2
```

## 📊 Check Status

```powershell
# See what's been created
./commands/workflow/workflow-status.ps1 -VisionEpicId YOUR_VISION_ID
```

## 💡 Pro Tips

1. **Preview First**: Add `-Preview` to any command to see without creating
2. **Skip Approvals**: Add `-AutoApprove` to any command
3. **Limit Items**: Use `-Max*` parameters (e.g., `-MaxEpics 3`)
4. **Track IDs**: Each command shows created IDs - note them!

## 🔄 Common Workflow Pattern

```
Vision (1)
  └── Strategy (1)
      └── Epics (3)
          └── Features (3 each = 9 total)
              └── Stories (3 each = 27 total)
                  ├── Test Cases (6 each = 162 total)
                  └── Tasks (6 each = 162 total)
```

## ⚡ Speed Run Commands (Auto-Approve)

```powershell
# WARNING: Creates everything without prompts!
./commands/vision/create-product-vision.ps1 -VisionName "Speed Test" -AutoApprove
./commands/strategy/build-vision-strategy-enhanced.ps1 -VisionEpicId ID -AutoApprove
./commands/epics/build-epics.ps1 -StrategyEpicId ID -MaxEpics 2 -AutoApprove
# ... continue pattern
```