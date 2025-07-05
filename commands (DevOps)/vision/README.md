# Product Vision Commands

Commands for creating and managing Product Vision Epics in Azure DevOps.

## create-product-vision.ps1

Creates a comprehensive Product Vision Epic that serves as the foundation for all subsequent product development work.

### Usage

```powershell
./create-product-vision.ps1 -VisionName "Your Product Name" [-Preview] [-AutoApprove]
```

### Parameters

- **-VisionName** (Required): Name of the product/system
- **-Preview**: Show what would be created without making changes
- **-AutoApprove**: Skip approval prompt and create immediately

### Features

1. **Vision Analysis**
   - Extracts strategic themes from product name
   - Identifies key metrics and success criteria
   - Checks for completeness

2. **Content Generation**
   - Executive Summary
   - Problem Statement
   - Target Market & Users
   - Core Value Propositions
   - Success Metrics & KPIs
   - Strategic Themes
   - Technical Architecture
   - Competitive Analysis
   - Risk Assessment
   - 5-Year Roadmap Vision

3. **Approval Workflow**
   - Shows vision analysis
   - Previews content structure
   - Requires confirmation before creation

4. **Audit Trail**
   - Saves creation details to `/docs/visions/`
   - Includes timestamp and creator

### Example

```powershell
# Preview mode
./create-product-vision.ps1 -VisionName "AI-Powered HR Platform" -Preview

# Create with approval
./create-product-vision.ps1 -VisionName "AI-Powered HR Platform"

# Auto-create (skip approval)
./create-product-vision.ps1 -VisionName "AI-Powered HR Platform" -AutoApprove
```

### Output

- Creates Epic with title: "**Product Vision – Your Product Name"
- Tags: "Product Vision", "Strategic", product name
- Links to Azure DevOps URL
- Saves audit JSON with Epic ID

### Next Steps

After creating a Product Vision:
1. Review and refine the vision content in Azure DevOps
2. Run `build-vision-strategy-enhanced.ps1` to create the Vision Strategy
3. Continue with the workflow to create Epics, Features, and Stories