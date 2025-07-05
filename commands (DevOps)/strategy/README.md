# Vision Strategy Commands

Commands for creating Vision Strategy Epics that define the 12-18 month execution plan for a Product Vision.

## build-vision-strategy-enhanced.ps1

Analyzes a Product Vision Epic and generates a comprehensive Vision Strategy with roadmap, initiatives, and success metrics.

### Usage

```powershell
./build-vision-strategy-enhanced.ps1 -VisionEpicId <ID> [-TimeframeMonths 18] [-Preview] [-AutoApprove]
```

### Parameters

- **-VisionEpicId** (Required): ID of the Product Vision Epic
- **-TimeframeMonths**: Strategy timeframe (default: 18)
- **-Preview**: Show what would be created without making changes
- **-AutoApprove**: Skip approval prompt and create immediately

### Features

1. **Vision Analysis**
   - Extracts strategic themes from Product Vision
   - Identifies success metrics and KPIs
   - Analyzes market opportunities

2. **Strategy Generation**
   - Strategic Objectives & Key Results
   - Quarterly Initiatives (Q1-Q6)
   - Implementation Roadmap
   - Resource Requirements
   - Risk Mitigation Plans
   - Success Metrics

3. **Intelligent Content Extraction**
   - Parses Product Vision HTML content
   - Maintains context from parent Epic
   - Aligns with vision themes

### Example

```powershell
# Preview strategy for vision
./build-vision-strategy-enhanced.ps1 -VisionEpicId 10087 -Preview

# Create 18-month strategy
./build-vision-strategy-enhanced.ps1 -VisionEpicId 10087

# Create 12-month strategy with auto-approval
./build-vision-strategy-enhanced.ps1 -VisionEpicId 10087 -TimeframeMonths 12 -AutoApprove
```

### Output

- Creates Epic with title: "**Vision Strategy – Product Name (Next 18 Months)"
- Links as child of Product Vision Epic
- Tags: "Vision Strategy", "Roadmap", product name
- Quarterly initiative breakdown

### Next Steps

After creating a Vision Strategy:
1. Review quarterly initiatives in Azure DevOps
2. Run `build-epics.ps1` to generate implementation Epics
3. Prioritize initiatives based on business value