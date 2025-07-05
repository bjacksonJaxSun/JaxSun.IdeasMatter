# Build Check Script for Jackson.Ideas Solution
Write-Host "Jackson.Ideas Build Check Script" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet is available
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found! Please install .NET 9 SDK." -ForegroundColor Red
    exit 1
}

# Clean previous build artifacts
Write-Host ""
Write-Host "Cleaning previous build artifacts..." -ForegroundColor Yellow
dotnet clean Jackson.Ideas.sln --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Clean completed successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Clean failed" -ForegroundColor Red
    exit 1
}

# Restore packages
Write-Host ""
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore Jackson.Ideas.sln --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Package restore completed successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Package restore failed" -ForegroundColor Red
    exit 1
}

# Build the solution
Write-Host ""
Write-Host "Building solution..." -ForegroundColor Yellow
$buildOutput = dotnet build Jackson.Ideas.sln --configuration Debug --no-restore 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Build completed successfully!" -ForegroundColor Green
    Write-Host ""
    
    # Count warnings
    $warnings = ($buildOutput | Select-String "warning" | Measure-Object).Count
    if ($warnings -gt 0) {
        Write-Host "⚠ Build completed with $warnings warning(s)" -ForegroundColor Yellow
    } else {
        Write-Host "✓ No warnings found" -ForegroundColor Green
    }
    
    # Show build summary
    Write-Host ""
    Write-Host "Build Summary:" -ForegroundColor Cyan
    Write-Host "=============" -ForegroundColor Cyan
    
    # Extract project build results
    $buildOutput | Where-Object { $_ -match "Done Building Project|Build succeeded|Build FAILED" } | ForEach-Object {
        if ($_ -match "FAILED") {
            Write-Host $_ -ForegroundColor Red
        } elseif ($_ -match "succeeded") {
            Write-Host $_ -ForegroundColor Green
        } else {
            Write-Host $_
        }
    }
    
} else {
    Write-Host "✗ Build failed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error Details:" -ForegroundColor Red
    Write-Host "=============" -ForegroundColor Red
    
    # Show errors
    $buildOutput | Where-Object { $_ -match "error" } | ForEach-Object {
        Write-Host $_ -ForegroundColor Red
    }
    
    exit 1
}

# Run tests if build succeeded
Write-Host ""
Write-Host "Running tests..." -ForegroundColor Yellow
$testOutput = dotnet test Jackson.Ideas.sln --configuration Debug --no-build --verbosity quiet 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ All tests passed!" -ForegroundColor Green
    
    # Extract test summary
    $testSummary = $testOutput | Where-Object { $_ -match "Passed|Failed|Skipped|Total" }
    if ($testSummary) {
        Write-Host ""
        Write-Host "Test Summary:" -ForegroundColor Cyan
        $testSummary | ForEach-Object { Write-Host $_ }
    }
} else {
    Write-Host "✗ Some tests failed!" -ForegroundColor Red
    
    # Show failed tests
    $testOutput | Where-Object { $_ -match "Failed" } | ForEach-Object {
        Write-Host $_ -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Build check completed!" -ForegroundColor Cyan

# Final status
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Solution is ready for development!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "✗ Please fix the issues above before proceeding." -ForegroundColor Red
    exit 1
}