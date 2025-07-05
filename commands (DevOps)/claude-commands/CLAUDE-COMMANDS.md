# Claude Natural Language Commands

## 🎯 Intuitive Command Format

All commands follow natural language patterns that Claude can easily understand and execute.

## 📝 Individual Step Commands

### 1. Create Vision
```bash
./Create-Vision.ps1 "Employee Portal"
./Create-Vision.ps1 "HR Management System"
```

### 2. Create Strategy
```bash
./Create-Strategy.ps1 10123
# Uses the Vision Epic ID from step 1
```

### 3. Create Epics
```bash
./Create-Epics.ps1 10124          # Creates up to 20 epics
./Create-Epics.ps1 10124 3        # Creates only 3 epics
# Uses the Strategy Epic ID from step 2
```

### 4. Create Features
```bash
./Create-Features.ps1 10125       # Creates up to 40 features
./Create-Features.ps1 10125 5     # Creates only 5 features
# Uses an Epic ID from step 3
```

### 5. Create Stories
```bash
./Create-Stories.ps1 10128        # Creates up to 40 stories
./Create-Stories.ps1 10128 3      # Creates only 3 stories
# Uses a Feature ID from step 4
```

### 6. Create Test Cases
```bash
./Create-TestCases.ps1 10131      # Creates up to 40 test cases
./Create-TestCases.ps1 10131 6    # Creates only 6 test cases
# Uses a Story ID from step 5
```

### 7. Create Tasks
```bash
./Create-Tasks.ps1 10131          # Creates up to 40 tasks
./Create-Tasks.ps1 10131 6        # Creates only 6 tasks
# Uses a Story ID from step 5
```

### 8. Design Story
```bash
./Design-Story.ps1 10131          # Creates technical design and attaches to story
./Design-Story.ps1 10131 -Preview # Preview design without attaching
# Uses a Story ID to analyze tasks and test cases

# Features:
# - Generates comprehensive technical design document
# - Creates Mermaid class diagrams following TECHNICAL_GUIDELINES.md
# - Extracts entities, services, repositories, and controllers from tasks
# - Shows inheritance, interfaces, and dependencies
# - Attaches HTML and Markdown versions to Azure DevOps
```

## 🚀 Full Workflow Command

### Create Everything at Once
```bash
# Default limits (5 epics, 5 features each, etc.)
./Create-FullWorkflow.ps1 "Employee Portal"

# Custom limits: 2 epics, 3 features per epic, 2 stories per feature
./Create-FullWorkflow.ps1 "Employee Portal" 2 3 2
```

## 🔄 Update Existing Test Cases
```bash
# Update single test case
./Update-TestCases.ps1 10103

# Update multiple test cases
./Update-TestCases.ps1 10103,10104,10105,10106
```

## 📊 Check Status
```bash
# Check all workflows
./Check-Status.ps1

# Check specific vision
./Check-Status.ps1 10123
```

## 👁️ Preview Mode

Add `-Preview` to any command to see what would be created:

```bash
./Create-Vision.ps1 "Test Product" -Preview
./Create-Epics.ps1 10124 3 -Preview
./Create-FullWorkflow.ps1 "Test Product" 2 3 2 -Preview
```

## 💬 Example Claude Conversation

```
User: "Create a workflow for Customer Support Portal with 2 epics and 3 features each"

Claude: I'll create the Customer Support Portal workflow for you:

<bash>
./Create-FullWorkflow.ps1 "Customer Support Portal" 2 3 3
</bash>

This will create:
- 1 Product Vision
- 1 Vision Strategy
- 2 Epics
- 6 Features (3 per Epic)
- 18 Stories (3 per Feature)
- Test Cases and Tasks for each Story
```

## 📋 Command Reference

| Command | Usage | Description |
|---------|-------|-------------|
| `Create-Vision.ps1` | `"Product Name"` | Creates Product Vision |
| `Create-Strategy.ps1` | `VisionID` | Creates Vision Strategy |
| `Create-Epics.ps1` | `StrategyID [Count]` | Creates Epics |
| `Create-Features.ps1` | `EpicID [Count]` | Creates Features |
| `Create-Stories.ps1` | `FeatureID [Count]` | Creates User Stories |
| `Create-TestCases.ps1` | `StoryID [Count]` | Creates Test Cases with steps |
| `Create-Tasks.ps1` | `StoryID [Count]` | Creates Tasks |
| `Design-Story.ps1` | `StoryID [-Preview]` | Creates technical design with class diagrams |
| `Create-FullWorkflow.ps1` | `"Name" [E] [F] [S]` | Creates complete hierarchy |
| `Update-TestCases.ps1` | `ID1,ID2,ID3` | Adds steps to test cases |
| `Check-Status.ps1` | `[VisionID]` | Shows workflow status |

## 🎨 Visual Indicators

All commands use emojis for clear visual feedback:
- 🎯 Target/Goal
- 📊 Data/Statistics
- ✅ Success/Approval
- 📋 Preview/Information
- 💡 Next Steps
- ⚡ Action/Task
- 🧪 Testing
- 🚀 Launch/Start
- ✨ Complete/Success
- 📐 Design/Architecture

## 💡 Tips

1. **Positional Parameters**: Most commands use positional parameters for natural flow
2. **Optional Counts**: Second parameter is usually the max count
3. **Auto-Approval**: All commands automatically approve creation
4. **Progress Feedback**: Commands show what they're doing with clear messages
5. **Next Steps**: Commands suggest the next logical command to run