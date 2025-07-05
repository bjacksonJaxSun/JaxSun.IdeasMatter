# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Ideas Matter is an AI-powered idea development platform that transforms concepts into deployable code. The system conducts market research, creates business plans, generates technical architecture, and produces production-ready applications.

**Tech Stack:**
- Frontend: Blazor Server (.NET 8)
- Backend: ASP.NET Core Web API (.NET 8) 
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

# Run tests
dotnet test
```

### Build Error Resolution Protocol
**CRITICAL**: Always check for and resolve build errors before completing any task.

```bash
# Always run build check after making changes
dotnet build

# Fix any compilation errors before proceeding
# Check for missing using statements
# Verify correct package versions
# Ensure all dependencies are referenced
```

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

3. **Quality Assurance:**
   - Build solution until all compilation errors are resolved
   - Run all tests until 100% pass rate achieved
   - Fix any failing tests and implementation issues
   - Verify integration points work correctly
   - Validate against phase acceptance criteria

#### 3. Completion Criteria
A phase is considered complete when:
- ✅ Clean build with zero errors or warnings
- ✅ All tests passing (100% success rate)
- ✅ Code coverage meets 80% threshold
- ✅ All phase requirements implemented
- ✅ Integration tests validate end-to-end functionality
- ✅ Documentation updated for new features

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

- Always run `dotnet build` before committing code
- Ensure all tests pass before submitting pull requests
- Follow semantic versioning for releases
- Update documentation for any architectural changes
- Use feature flags for experimental functionality
- Implement proper security practices for all endpoints

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