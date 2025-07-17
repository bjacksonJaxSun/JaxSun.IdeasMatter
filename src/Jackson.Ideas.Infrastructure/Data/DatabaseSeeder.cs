using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly JacksonIdeasDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        JacksonIdeasDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedAIProviderConfigsAsync();
            await SeedSampleDataAsync();
            await SeedResearchSessionsAsync();

            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Seeding roles...");

        var roles = new[] { "User", "Admin", "SystemAdmin" };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                var result = await _roleManager.CreateAsync(role);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    _logger.LogError("Failed to create role: {RoleName}. Errors: {Errors}", 
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        _logger.LogInformation("Seeding users...");

        // Seed Admin User
        await SeedAdminUserAsync();
        
        // Seed Regular Users
        await SeedRegularUsersAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@jackson.ideas";
        const string adminPassword = "Admin@123456";
        
        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "System Administrator",
                EmailConfirmed = true,
                Role = UserRole.SystemAdmin,
                IsActive = true,
                IsVerified = true,
                AuthProvider = "local",
                CreatedAt = DateTime.UtcNow,
                Permissions = "[\"*\"]" // Full permissions
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "SystemAdmin");
                _logger.LogInformation("Created admin user: {Email}", adminEmail);
            }
            else
            {
                _logger.LogError("Failed to create admin user. Errors: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            _logger.LogInformation("Admin user already exists: {Email}", adminEmail);
        }
    }

    private async Task SeedRegularUsersAsync()
    {
        var users = new[]
        {
            new { Email = "user1@jackson.ideas", Name = "John Doe", Role = UserRole.User },
            new { Email = "user2@jackson.ideas", Name = "Jane Smith", Role = UserRole.User },
            new { Email = "manager@jackson.ideas", Name = "Mike Manager", Role = UserRole.Admin }
        };

        foreach (var userData in users)
        {
            var existingUser = await _userManager.FindByEmailAsync(userData.Email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userData.Email,
                    Email = userData.Email,
                    Name = userData.Name,
                    EmailConfirmed = true,
                    Role = userData.Role,
                    IsActive = true,
                    IsVerified = true,
                    AuthProvider = "local",
                    CreatedAt = DateTime.UtcNow,
                    Permissions = userData.Role == UserRole.Admin ? "[\"research:*\", \"user:read\"]" : "[\"research:read\", \"research:create\"]"
                };

                var result = await _userManager.CreateAsync(user, "User@123456");
                if (result.Succeeded)
                {
                    var roleName = userData.Role == UserRole.Admin ? "Admin" : "User";
                    await _userManager.AddToRoleAsync(user, roleName);
                    _logger.LogInformation("Created user: {Email} with role {Role}", userData.Email, roleName);
                }
                else
                {
                    _logger.LogError("Failed to create user {Email}. Errors: {Errors}", 
                        userData.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private async Task SeedAIProviderConfigsAsync()
    {
        _logger.LogInformation("Seeding AI provider configurations...");

        if (!await _context.Set<AIProviderConfig>().AnyAsync())
        {
            var configs = new[]
            {
                new AIProviderConfig
                {
                    Name = "OpenAI GPT-4",
                    ProviderType = AIProviderType.OpenAI,
                    ApiKey = "demo-openai-key",
                    BaseUrl = "https://api.openai.com/v1",
                    ModelName = "gpt-4",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new AIProviderConfig
                {
                    Name = "Claude 3 Sonnet",
                    ProviderType = AIProviderType.Claude,
                    ApiKey = "demo-claude-key",
                    BaseUrl = "https://api.anthropic.com",
                    ModelName = "claude-3-sonnet-20240229",
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Set<AIProviderConfig>().AddRange(configs);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} AI provider configurations", configs.Length);
        }
    }

    private async Task SeedSampleDataAsync()
    {
        _logger.LogInformation("Seeding sample research data...");

        if (!await _context.Set<Research>().AnyAsync())
        {
            var adminUser = await _userManager.FindByEmailAsync("admin@jackson.ideas");
            var regularUser = await _userManager.FindByEmailAsync("user1@jackson.ideas");

            if (adminUser != null && regularUser != null)
            {
                var sampleResearches = new[]
                {
                    new Research
                    {
                        Title = "AI-Powered Healthcare Management System",
                        Description = "Research for developing an AI-powered system to manage healthcare operations and patient data.",
                        UserId = adminUser.Id,
                        Status = ResearchStatus.Completed,
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        UpdatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new Research
                    {
                        Title = "E-commerce Personalization Engine",
                        Description = "Market research for building a personalized shopping experience using ML algorithms.",
                        UserId = regularUser.Id,
                        Status = ResearchStatus.InProgress,
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        UpdatedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new Research
                    {
                        Title = "Sustainable Energy Dashboard",
                        Description = "Research into renewable energy monitoring and optimization solutions.",
                        UserId = regularUser.Id,
                        Status = ResearchStatus.Pending,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    }
                };

                _context.Set<Research>().AddRange(sampleResearches);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded {Count} sample research records", sampleResearches.Length);
            }
        }
    }

    private async Task SeedResearchSessionsAsync()
    {
        _logger.LogInformation("Seeding sample research sessions...");

        if (!await _context.Set<ResearchSession>().AnyAsync())
        {
            var adminUser = await _userManager.FindByEmailAsync("admin@jackson.ideas");
            var regularUser = await _userManager.FindByEmailAsync("user1@jackson.ideas");
            var user2 = await _userManager.FindByEmailAsync("user2@jackson.ideas");

            if (adminUser != null && regularUser != null && user2 != null)
            {
                var sampleSessions = new[]
                {
                    new ResearchSession
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminUser.Id,
                        Title = "AI-Powered Customer Service Chatbot",
                        Description = "Market research for developing an intelligent customer service chatbot using natural language processing and machine learning.",
                        Status = ResearchStatus.Completed,
                        ResearchApproach = "comprehensive",
                        ResearchType = "Market Analysis",
                        Goals = "Analyze market demand, competition, and technical requirements for AI chatbot solution",
                        EstimatedDurationMinutes = 45,
                        ProgressPercentage = 100.0,
                        CurrentPhase = "Completed",
                        StartedAt = DateTime.UtcNow.AddDays(-3),
                        CompletedAt = DateTime.UtcNow.AddDays(-1),
                        AnalysisConfidence = 92.5,
                        AnalysisCompleteness = 98.0,
                        Strategy = "Deep Market Analysis",
                        EstimatedCompletionTime = 45,
                        ConfidenceScore = 93,
                        Industry = "Technology / Customer Service",
                        TargetMarket = "Small to medium businesses with customer service needs",
                        PrimaryGoal = "Reduce customer service costs by 40% through AI automation",
                        NextSteps = @"[
                            ""Develop technical architecture specification"",
                            ""Create MVP prototype with basic NLP capabilities"",
                            ""Establish partnerships with cloud AI providers"",
                            ""Design user interface for customer interaction"",
                            ""Plan pilot program with 5 beta customers""
                        ]",
                        CreatedAt = DateTime.UtcNow.AddDays(-4),
                        UpdatedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new ResearchSession
                    {
                        Id = Guid.NewGuid(),
                        UserId = regularUser.Id,
                        Title = "Sustainable Food Delivery Service",
                        Description = "Research into eco-friendly food delivery platform focusing on local restaurants and sustainable packaging.",
                        Status = ResearchStatus.InProgress,
                        ResearchApproach = "quick_validation",
                        ResearchType = "Quick Validation",
                        Goals = "Validate market demand for sustainable delivery options and identify key success factors",
                        EstimatedDurationMinutes = 20,
                        ProgressPercentage = 65.0,
                        CurrentPhase = "Competitive Analysis",
                        StartedAt = DateTime.UtcNow.AddHours(-2),
                        AnalysisConfidence = 78.0,
                        AnalysisCompleteness = 65.0,
                        Strategy = "Quick Market Validation",
                        EstimatedCompletionTime = 20,
                        ConfidenceScore = 78,
                        Industry = "Food & Beverage / Logistics",
                        TargetMarket = "Environmentally conscious consumers in urban areas",
                        PrimaryGoal = "Launch sustainable delivery service in 3 metropolitan areas",
                        NextSteps = @"[
                            ""Complete competitive analysis research"",
                            ""Survey potential customers on sustainability preferences"",
                            ""Research sustainable packaging suppliers"",
                            ""Analyze delivery route optimization""
                        ]",
                        CreatedAt = DateTime.UtcNow.AddHours(-3),
                        UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
                    },
                    new ResearchSession
                    {
                        Id = Guid.NewGuid(),
                        UserId = user2.Id,
                        Title = "Remote Work Productivity Platform",
                        Description = "Research for developing a comprehensive platform to enhance remote work productivity and team collaboration.",
                        Status = ResearchStatus.Pending,
                        ResearchApproach = "launch_strategy",
                        ResearchType = "Launch Strategy",
                        Goals = "Develop go-to-market strategy for remote work productivity tools targeting distributed teams",
                        EstimatedDurationMinutes = 60,
                        ProgressPercentage = 0.0,
                        CurrentPhase = "Queued",
                        Strategy = "Comprehensive Launch Strategy",
                        EstimatedCompletionTime = 60,
                        ConfidenceScore = 0,
                        Industry = "Software / Remote Work",
                        TargetMarket = "Remote teams and distributed companies",
                        PrimaryGoal = "Increase remote team productivity by 30% through integrated collaboration tools",
                        NextSteps = @"[
                            ""Begin initial market research"",
                            ""Identify key competitors and their offerings"",
                            ""Analyze remote work trends and pain points"",
                            ""Define target customer segments""
                        ]",
                        CreatedAt = DateTime.UtcNow.AddHours(-1)
                    },
                    new ResearchSession
                    {
                        Id = Guid.NewGuid(),
                        UserId = regularUser.Id,
                        Title = "Smart Home Energy Management",
                        Description = "Market analysis for IoT-based smart home energy monitoring and optimization system.",
                        Status = ResearchStatus.Failed,
                        ResearchApproach = "comprehensive",
                        ResearchType = "Market Analysis",
                        Goals = "Assess market potential for smart home energy solutions and identify key technical challenges",
                        EstimatedDurationMinutes = 50,
                        ProgressPercentage = 25.0,
                        CurrentPhase = "Failed",
                        StartedAt = DateTime.UtcNow.AddDays(-2),
                        ErrorMessage = "AI service temporarily unavailable. Research will be retried automatically.",
                        Strategy = "Deep Market Analysis",
                        EstimatedCompletionTime = 50,
                        ConfidenceScore = 0,
                        Industry = "IoT / Smart Home",
                        TargetMarket = "Homeowners interested in energy efficiency and smart home automation",
                        PrimaryGoal = "Reduce home energy consumption by 25% through intelligent monitoring",
                        NextSteps = @"[
                            ""Retry research session when AI services are available"",
                            ""Review technical requirements for IoT integration"",
                            ""Analyze smart home market trends""
                        ]",
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        UpdatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new ResearchSession
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminUser.Id,
                        Title = "Educational VR Platform for Medical Training",
                        Description = "Research into virtual reality platform for medical education and surgical training simulation.",
                        Status = ResearchStatus.Completed,
                        ResearchApproach = "launch_strategy",
                        ResearchType = "Launch Strategy",
                        Goals = "Develop comprehensive launch strategy for VR medical training platform targeting medical schools",
                        EstimatedDurationMinutes = 55,
                        ProgressPercentage = 100.0,
                        CurrentPhase = "Completed",
                        StartedAt = DateTime.UtcNow.AddDays(-7),
                        CompletedAt = DateTime.UtcNow.AddDays(-5),
                        AnalysisConfidence = 89.0,
                        AnalysisCompleteness = 95.0,
                        Strategy = "Comprehensive Launch Strategy",
                        EstimatedCompletionTime = 55,
                        ConfidenceScore = 89,
                        Industry = "Healthcare / Education Technology",
                        TargetMarket = "Medical schools, hospitals, and healthcare training institutions",
                        PrimaryGoal = "Improve medical training outcomes by 40% through immersive VR simulations",
                        NextSteps = @"[
                            ""Develop partnerships with medical schools"",
                            ""Create VR content for common surgical procedures"",
                            ""Build strategic partnerships with VR hardware vendors"",
                            ""Design clinical validation studies"",
                            ""Establish regulatory compliance framework""
                        ]",
                        CreatedAt = DateTime.UtcNow.AddDays(-8),
                        UpdatedAt = DateTime.UtcNow.AddDays(-5)
                    }
                };

                _context.Set<ResearchSession>().AddRange(sampleSessions);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded {Count} sample research sessions", sampleSessions.Length);
            }
        }
    }
}