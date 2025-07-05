# Workflow Orchestration Commands

Master commands for orchestrating and monitoring the complete product development workflow.

## execute-full-workflow.ps1

Orchestrates the entire product development workflow from Vision to Tasks with approval gates at each level.

### Usage

```powershell
# Start from Vision
./execute-full-workflow.ps1 -VisionName "Product Name" [-StopAt Tasks] [-AutoApprove] [-DryRun]

# Start from existing Epic
./execute-full-workflow.ps1 -StartFrom Epic -ParentId 10089 [-StopAt Stories]
```

### Parameters

- **-StartFrom**: Where to start (Vision, Strategy, Epic, Feature, Story)
- **-VisionName**: Required if starting from Vision
- **-ParentId**: Required if not starting from Vision
- **-StopAt**: Where to stop (Vision, Strategy, Epics, Features, Stories, TestCases, Tasks)
- **-AutoApprove**: Skip all approval prompts
- **-DryRun**: Execute in preview mode
- **-MaxItemsPerLevel**: Limit items created at each level (default: 10)

### Features

1. **Flexible Entry Points**
   - Start from any level in the hierarchy
   - Resume interrupted workflows
   - Partial execution support

2. **Approval Gates**
   - Confirmation before each phase
   - Option to skip phases
   - Preview what will be created

3. **Progress Tracking**
   - Real-time execution logs
   - Created item tracking
   - Error and warning collection

4. **Workflow Report**
   - Execution summary
   - Created items by type
   - Duration tracking
   - JSON report saved

### Example Workflows

```powershell
# Full workflow with approvals
./execute-full-workflow.ps1 -VisionName "AI Analytics Platform"

# Dry run to preview everything
./execute-full-workflow.ps1 -VisionName "AI Analytics Platform" -DryRun

# Create only Vision and Strategy
./execute-full-workflow.ps1 -VisionName "HR Platform" -StopAt Strategy

# Resume from existing Feature
./execute-full-workflow.ps1 -StartFrom Feature -ParentId 10095 -StopAt Tasks
```

## workflow-status.ps1

Analyzes Azure DevOps backlog to show workflow progress and identify bottlenecks.

### Usage

```powershell
./workflow-status.ps1 [-ProductName "Name"] [-ShowDetails] [-ExportReport]
```

### Parameters

- **-ProductName**: Filter by specific product
- **-ShowDetails**: Show detailed work item breakdown
- **-ExportReport**: Export HTML report

### Features

1. **Progress Analysis**
   - Completion percentages by level
   - Visual progress bars
   - Item counts and states

2. **Bottleneck Detection**
   - Identifies levels below 50% completion
   - Shows pending item counts
   - Prioritization guidance

3. **Health Score**
   - Overall workflow health percentage
   - Color-coded status indicators
   - Trend analysis

4. **HTML Reports**
   - Professional report format
   - Charts and visualizations
   - Shareable status updates

### Example

```powershell
# Basic status check
./workflow-status.ps1

# Detailed status for specific product
./workflow-status.ps1 -ProductName "Bermuda Payroll" -ShowDetails

# Generate HTML report
./workflow-status.ps1 -ExportReport
```

### Output

```
=== PRODUCT DEVELOPMENT WORKFLOW STATUS ===
Generated: 2024-01-15 10:30
===========================================

WORKFLOW SUMMARY:
─────────────────────────────────────────────────────────────────
Visions      [████████████████████████████░░] 93.3% 14/15
Strategies   [████████████████████░░░░░░░░░░] 66.7% 10/15
Epics        [██████████████░░░░░░░░░░░░░░░░] 45.2% 47/104
Features     [██████████░░░░░░░░░░░░░░░░░░░░] 35.8% 89/248
Stories      [████████░░░░░░░░░░░░░░░░░░░░░░] 25.1% 125/498
TestCases    [██████░░░░░░░░░░░░░░░░░░░░░░░░] 18.9% 94/497
Tasks        [████░░░░░░░░░░░░░░░░░░░░░░░░░░] 12.3% 61/496

WORKFLOW HEALTH SCORE: 42.8%
```

## Best Practices

1. **Start Small**: Use `-MaxItemsPerLevel 5` for initial runs
2. **Use Dry Run**: Always preview with `-DryRun` first
3. **Incremental Execution**: Stop at each level to review
4. **Regular Monitoring**: Run status checks weekly
5. **Export Reports**: Share HTML reports with stakeholders

## Troubleshooting

### Workflow Stops Unexpectedly
- Check error messages in console
- Review workflow report JSON
- Verify parent work items exist

### Performance Issues
- Reduce `-MaxItemsPerLevel`
- Execute in phases using `-StopAt`
- Run during off-peak hours

### Missing Work Items
- Ensure proper parent-child linking
- Check work item permissions
- Verify area/iteration paths