# Deployment Strategy & Validation Plan

Since we don't have Docker/dotnet available in this environment, here's a systematic approach to validate and deploy our fixes.

## Problem Analysis

### Root Cause of 404 Error
1. **Static files not being published** - The `wwwroot` directory isn't included in Docker publish output
2. **Incorrect .NET 9 configuration** - Missing static web assets configuration
3. **Docker build process** - Not properly including static files in final image

## Fixes Applied

### 1. Project Configuration Updates
**File: `src/Jackson.Ideas.Web/Jackson.Ideas.Web.csproj`**

Added:
```xml
<PropertyGroup>
  <GenerateStaticWebAssetsManifest>true</GenerateStaticWebAssetsManifest>
  <StaticWebAssetsEnabled>true</StaticWebAssetsEnabled>
</PropertyGroup>

<ItemGroup>
  <Content Include="wwwroot\**\*" CopyToPublishDirectory="PreserveNewest" />
</ItemGroup>
```

**Purpose**: Ensures .NET 9 properly includes static web assets in publish output.

### 2. Docker Build Process Updates
**File: `Dockerfile.render`**

Changed:
```dockerfile
RUN dotnet publish "Jackson.Ideas.Web.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishSingleFile=false \
    /p:PublishReadyToRun=false
```

**Purpose**: Ensures proper .NET 9 publish parameters for web applications.

### 3. Program.cs Cleanup
**File: `src/Jackson.Ideas.Web/Program.cs`**

Reverted to modern .NET 9 approach:
```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
```

**Purpose**: Uses correct .NET 9 Blazor Server configuration.

## Validation Strategy

### Phase 1: Controlled Deployment Test
Since we can't test locally, we'll deploy incrementally:

1. **Deploy current changes**
2. **Monitor build logs on Render** for errors
3. **Test specific endpoints systematically**
4. **Analyze failures and iterate**

### Phase 2: Systematic Testing
Test endpoints in this order:

1. **Health check**: `https://jaxsun-ideasmatter.onrender.com/health`
   - Should return: `{"status": "healthy", "timestamp": "..."}`
   - Status: Known working ✅

2. **Static CSS**: `https://jaxsun-ideasmatter.onrender.com/app.css`
   - Should return: CSS content with 200 status
   - Current status: 404 ❌ (this is what we're fixing)

3. **Static favicon**: `https://jaxsun-ideasmatter.onrender.com/favicon.png`
   - Should return: PNG image with 200 status
   - Current status: 404 ❌

4. **Blazor framework**: `https://jaxsun-ideasmatter.onrender.com/_framework/blazor.web.js`
   - Should return: JavaScript file with 200 status
   - Current status: 404 ❌

5. **Main page**: `https://jaxsun-ideasmatter.onrender.com/`
   - Should return: HTML page with 200 status
   - Current status: 404 ❌

### Phase 3: Fallback Plans

If current fixes don't work:

#### Plan B: Manual wwwroot Copy
Add to Dockerfile:
```dockerfile
# Manually copy wwwroot after publish
COPY src/Jackson.Ideas.Web/wwwroot /app/wwwroot
```

#### Plan C: Simplified Docker Approach
Create a minimal Dockerfile:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY . .
RUN dotnet restore && dotnet publish -o /app/publish
WORKDIR /app/publish
ENTRYPOINT ["dotnet", "Jackson.Ideas.Web.dll"]
```

#### Plan D: Different Base Image
Try using the SDK image instead of aspnet:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0
```

## Implementation Steps

### Step 1: Deploy Current Fixes
```bash
git add -A
git commit -m "Fix .NET 9 static web assets configuration"
git push
```

### Step 2: Monitor Deployment
1. Watch Render build logs
2. Check for compilation errors
3. Note any new error messages

### Step 3: Test Systematically
Test each endpoint above and document results:
- ✅ Working
- ❌ 404 (not found)
- ⚠️ 500 (server error)
- 🔄 Other status

### Step 4: Iterate Based on Results
- If static files work: Success! ✅
- If still 404: Try Plan B (manual copy)
- If build fails: Fix compilation errors first

## Success Criteria

The deployment is successful when:
1. ✅ Health endpoint returns 200
2. ✅ CSS file loads (200 status, content-type: text/css)
3. ✅ Favicon loads (200 status, content-type: image/png)
4. ✅ Main page loads (200 status, content-type: text/html)
5. ✅ Blazor framework loads (200 status, content-type: application/javascript)

## Rollback Plan

If critical issues arise:
1. Revert to last working commit
2. Deploy known working state
3. Analyze issues offline
4. Try alternative approaches

## Local Development Setup (Future)

For team members with local environments:

1. **Install Prerequisites**:
   - .NET 9 SDK
   - Docker Desktop

2. **Use Provided Scripts**:
   ```bash
   chmod +x local-dev.sh docker-debug.sh test-local.sh
   ./test-local.sh all
   ```

3. **Validate Before Push**:
   - Local app runs: `./local-dev.sh run`
   - Docker builds: `./docker-debug.sh debug-build`
   - All tests pass: `./test-local.sh all`

This approach ensures we make informed decisions based on actual results rather than assumptions.