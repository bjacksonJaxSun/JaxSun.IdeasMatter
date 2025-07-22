# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Ideas Matter is an AI-powered idea development platform that transforms concepts into deployable code. The system conducts market research, creates business plans, generates technical architecture, and produces production-ready applications.

**Tech Stack:**
- Frontend: Blazor Server (.NET 9)
- Backend: ASP.NET Core Web API (.NET 9) 
- Database: Entity Framework Core with SQL Server/SQLite
- AI Integration: Multi-provider architecture with OpenAI, Claude, Azure OpenAI support

## Development Commands

### Quick Start
```bash
# Build solution
dotnet build

# Run API
dotnet run --project src/Jackson.Ideas.Api

# Run Blazor Web (when created)
dotnet run --project src/Jackson.Ideas.Web

# Run Mock Application  
dotnet run --project src/Jackson.Ideas.Mock

# Run tests
dotnet test

# Run Golden Rules validation script
./scripts/golden-rule.sh
```

### Build Error Resolution Protocol - GOLDEN RULE
**CRITICAL**: Before completing a set of tasks, run build, fix, build cycle until all errors are resolved.

**The Golden Rule Process:**
1. **Complete all planned code changes** in a task set
2. **Before marking tasks complete** → Run `dotnet build`
3. **If build fails** → Fix compilation errors immediately
4. **Repeat** → Run `dotnet build` again
5. **Continue** → Until clean build with zero errors
6. **Only then** → Mark task set as complete

```bash
# At the END of a task set, run build check
dotnet build

# Fix any compilation errors before proceeding
# Check for missing using statements
# Verify correct package versions
# Ensure all dependencies are referenced

# REPEAT until clean build
dotnet build  # Must show "Build succeeded" with 0 errors
```

**The Golden Rule applies before completing task sets, not after every individual change.**

### Golden Rules Automated Script
The repository includes an automated Golden Rules validation script that performs the complete build → fix → launch → validate cycle:

```bash
# Run the automated Golden Rules script
./scripts/golden-rule.sh

# The script will:
# 1. Build the solution (up to 5 attempts with auto-fix)
# 2. Launch the application on http://localhost:5000
# 3. Validate the application responds correctly
# 4. Report success/failure with detailed logs
# 5. Clean up processes automatically
```

**Golden Rules Script Features:**
- **Automated Build Validation**: Attempts to build and auto-fix common errors
- **Application Launch Testing**: Starts the app and verifies it's accessible
- **Response Validation**: Checks for error-free HTML output
- **Comprehensive Logging**: Saves build outputs and validation results to `/logs/`
- **Process Management**: Automatically cleans up running processes

**Important**: Always use the Golden Rules script before marking task sets complete. This ensures the build succeeds AND the application runs without errors.

### Testing Commands
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/Jackson.Ideas.Core.Tests/
```

## Development Workflows

### Phase Implementation Command

**Command:** `Implement Phase <phase_number>`

**Example Usage:**
- `Implement Phase 1`
- `Implement Phase 2`
- `Implement Phase 3`

**Workflow Process:**

#### 1. Analysis Phase
When this command is invoked, perform the following analysis:

1. **Read Documentation:**
   - Load and analyze `/docs/PRD.md`
   - Load and analyze `/docs/IMPLEMENTATION_PLAN.md`
   - Identify the specific phase requirements

2. **Status Assessment:**
   - Review current solution structure
   - Check existing implementations against phase requirements
   - Identify completed, in-progress, and pending items
   - Analyze dependencies and prerequisites

3. **Todo List Generation:**
   - Create comprehensive todo list for the phase
   - Break down complex items into specific, actionable tasks
   - Identify technical dependencies and order of implementation
   - Estimate complexity and priority for each item

4. **Technical Design Presentation:**
   - Present architectural decisions and patterns to be used
   - Outline file structure and class hierarchies
   - Describe integration points and interfaces
   - Explain testing strategy and approach
   - **IMPORTANT:** Wait for user approval before proceeding

#### 2. Implementation Phase (After User Approval)
Once user approves the technical design:

1. **Test-Driven Development (TDD) Approach:**
   - Create comprehensive test suite FIRST for all new functionality
   - Write unit tests for all services and business logic
   - Write integration tests for API endpoints
   - Write component tests for Blazor components (when applicable)
   - Ensure 80%+ code coverage target

2. **Implementation:**
   - Implement functionality to make tests pass
   - Follow SOLID principles and clean architecture patterns
   - Implement proper error handling and validation
   - Add comprehensive logging and monitoring
   - Follow .NET coding conventions and best practices

3. **Quality Assurance & Golden Rule Compliance:**
   - Complete all implementation work for the phase
   - **MANDATORY**: Apply Golden Rule before phase completion
   - Run `dotnet build` after completing all code changes in the phase
   - Fix ALL compilation errors before proceeding
   - Continue build-fix cycle until clean build achieved
   - Run all tests until 100% pass rate achieved
   - Fix any failing tests and implementation issues
   - Verify integration points work correctly
   - Validate against phase acceptance criteria
   - **FINAL BUILD CHECK**: Ensure `dotnet build` succeeds before marking phase complete

#### 3. Completion Criteria
A phase is considered complete when:
- ✅ **GOLDEN RULE SATISFIED**: Clean build with zero errors or warnings (`dotnet build` succeeds)
- ✅ All tests passing (100% success rate)
- ✅ Code coverage meets 80% threshold
- ✅ All phase requirements implemented
- ✅ Integration tests validate end-to-end functionality
- ✅ Documentation updated for new features
- ✅ **FINAL VERIFICATION**: One more `dotnet build` to confirm clean state

### Phase-Specific Guidelines

#### Phase 1: Foundation & Infrastructure
- Focus on solution architecture and database setup
- Implement authentication and security infrastructure
- Create base Blazor project with authentication
- Establish testing frameworks and CI/CD foundations

#### Phase 2: Core Business Logic
- Implement AI integration services
- Create research and analysis services
- Build business logic for idea validation and market analysis
- Establish real-time progress tracking

#### Phase 3: User Interface & Experience
- Create comprehensive Blazor components
- Implement responsive design and accessibility
- Build user workflows and navigation
- Integrate real-time updates with SignalR

#### Phase 4: Advanced Features & Integration
- Implement PDF generation and reporting
- Create data export functionality
- Add advanced real-time features
- Build integration points for external systems

#### Phase 5: Testing & Quality Assurance
- Comprehensive test coverage validation
- Performance testing and optimization
- Security testing and vulnerability assessment
- Documentation and code quality improvements

#### Phase 6: Deployment & DevOps
- CI/CD pipeline implementation
- Production deployment configuration
- Monitoring and alerting setup
- Performance monitoring and maintenance procedures

## Architecture Key Points

### Solution Structure
```
Jackson.Ideas/
├── src/
│   ├── Jackson.Ideas.Api/          # ASP.NET Core Web API
│   ├── Jackson.Ideas.Web/          # Blazor Server (Phase 1)
│   ├── Jackson.Ideas.Application/  # Business Logic Layer
│   ├── Jackson.Ideas.Core/         # Domain Models & Interfaces
│   ├── Jackson.Ideas.Infrastructure/ # Data Access & Services
│   └── Jackson.Ideas.Shared/       # Shared DTOs
├── tests/
│   ├── Jackson.Ideas.Api.Tests/
│   ├── Jackson.Ideas.Web.Tests/
│   ├── Jackson.Ideas.Application.Tests/
│   ├── Jackson.Ideas.Core.Tests/
│   └── Jackson.Ideas.Infrastructure.Tests/
└── docs/
    ├── PRD.md
    └── IMPLEMENTATION_PLAN.md
```

### Key Principles
- **Clean Architecture:** Separation of concerns with clear layer boundaries
- **SOLID Principles:** Maintainable and extensible code design
- **Test-Driven Development:** Tests written before implementation
- **Dependency Injection:** Loose coupling and testability
- **Async/Await:** Non-blocking operations throughout
- **Error Handling:** Comprehensive exception handling and logging

### Database Management
```bash
# Add migration
dotnet ef migrations add <MigrationName> --project src/Jackson.Ideas.Infrastructure

# Update database
dotnet ef database update --project src/Jackson.Ideas.Infrastructure

# Drop database (development only)
dotnet ef database drop --project src/Jackson.Ideas.Infrastructure
```

## Configuration

### Development Environment
- Uses SQLite for local development
- In-memory caching for development
- Console logging for debugging
- Development authentication with test users

### Production Environment
- SQL Server for production database
- Redis for distributed caching
- Application Insights for monitoring
- Azure AD or production OAuth providers

## Important Notes

- **GOLDEN RULE**: Always run `dotnet build` after code changes and fix all errors before proceeding
- Always run `dotnet build` before committing code
- Ensure all tests pass before submitting pull requests
- Follow semantic versioning for releases
- Update documentation for any architectural changes
- Use feature flags for experimental functionality
- Implement proper security practices for all endpoints

## Routing and Layout Protection

To prevent loss of page layouts and routing configurations:

### Validation Commands
```bash
# Validate routing and layout integrity
./scripts/validate-routing.sh

# Run routing tests
dotnet test tests/Jackson.Ideas.Web.Tests/ --filter "RoutingTests"

# Full validation (build + routing + tests)
dotnet build && ./scripts/validate-routing.sh && dotnet test
```

### Documentation References
- **Routing Guide**: `/docs/ROUTING_AND_LAYOUTS.md` - Complete routing and layout documentation
- **Route Constants**: `src/Jackson.Ideas.Web/Configuration/RouteConstants.cs` - Centralized route definitions

### Page Requirements Checklist
When adding/modifying pages:
- [ ] Add `@page` directive with unique route
- [ ] Specify layout: `@layout Layout.LandingLayout` or use default `MainLayout`
- [ ] Register required services in `Program.cs`
- [ ] Update navigation in `RouteConstants.cs` if user-facing
- [ ] Update `/docs/ROUTING_AND_LAYOUTS.md` documentation
- [ ] Run `./scripts/validate-routing.sh` to verify
- [ ] Apply Golden Rule: `dotnet build` must succeed

### Critical Files to Protect
Never modify these without updating documentation:
- `Components/Layout/MainLayout.razor` - Application layout
- `Components/Layout/LandingLayout.razor` - Landing page layout  
- `Components/Layout/NavMenu.razor` - Navigation menu
- `Components/Routes.razor` - Route configuration
- `Program.cs` - Service registrations

### Recovery Process
If routing is broken:
1. Check `/docs/ROUTING_AND_LAYOUTS.md` for correct configuration
2. Run `./scripts/validate-routing.sh` to identify issues
3. Verify `@page` directives match documented routes
4. Ensure services are registered in `Program.cs`
5. Test with `dotnet build` and manual navigation

## Task Management Golden Rule

**MANDATORY BEFORE COMPLETING TASK SETS:**

1. **Complete all planned code changes** in the task set
2. **Run `dotnet build`** before marking tasks complete
3. **If errors exist** → Fix them immediately
4. **Run `dotnet build` again**
5. **Repeat until clean build**
6. **Only then mark task set complete**

**This rule applies before completing:**
- Phase implementations
- Feature sets
- Bug fix batches
- Refactoring sessions
- Testing cycles

**A task set is NOT complete until the build succeeds with zero errors.**

## Troubleshooting

### Common Issues
1. **Build Errors:** Check package references and target frameworks
2. **Test Failures:** Verify database state and test isolation
3. **Authentication Issues:** Check JWT configuration and token validation
4. **AI Provider Issues:** Verify API keys and network connectivity

### Debug Commands
```bash
# Verbose build output
dotnet build --verbosity detailed

# Run specific test with debug info
dotnet test --logger "console;verbosity=detailed"

# Check package vulnerabilities
dotnet list package --vulnerable
```

This workflow ensures systematic, quality-driven development following the established implementation plan and maintaining high code quality standards throughout the project.