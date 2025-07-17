using Jackson.Ideas.Application.Services.AI;
using Jackson.Ideas.Application.Services;
using Jackson.Ideas.Core.Interfaces.AI;
using Jackson.Ideas.Core.Interfaces;
using Jackson.Ideas.Core.Interfaces.Services;
using Jackson.Ideas.Core.Configuration;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Infrastructure.Services.AI;
using Jackson.Ideas.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Jackson.Ideas.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IResearchRepository, ResearchRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        
        // Register AI services
        services.AddScoped<IAIOrchestrator, AIOrchestrator>();
        services.AddScoped<IAIProviderManager, AIProviderManager>();
        
        // Register mock data service
        services.AddSingleton<MockDataService>();
        
        // Register real services first (these have the actual implementations)
        services.AddScoped<MarketAnalysisService>();
        services.AddScoped<ResearchSessionService>();
        
        // Register interface implementations with demo mode support using factory pattern
        services.AddScoped<IMarketAnalysisService>(provider =>
        {
            var demoOptions = provider.GetRequiredService<IOptions<DemoModeOptions>>();
            var mockService = provider.GetRequiredService<MockDataService>();
            
            // Create real service manually to avoid circular dependency
            var aiService = provider.GetRequiredService<IAIOrchestrator>();
            var marketRepo = provider.GetRequiredService<IRepository<MarketAnalysis>>();
            var competitorRepo = provider.GetRequiredService<IRepository<CompetitorAnalysis>>();
            var researchRepo = provider.GetRequiredService<IRepository<Research>>();
            var logger = provider.GetRequiredService<ILogger<MarketAnalysisService>>();
            
            var realService = new MarketAnalysisService(aiService, marketRepo, competitorRepo, researchRepo, logger);
            
            return new DemoModeMarketAnalysisService(realService, mockService, demoOptions);
        });
        
        // Register the real ResearchSessionService first
        services.AddScoped<ResearchSessionService>();
        
        services.AddScoped<IResearchSessionService>(provider =>
        {
            var demoOptions = provider.GetRequiredService<IOptions<DemoModeOptions>>();
            var mockService = provider.GetRequiredService<MockDataService>();
            var realService = provider.GetRequiredService<ResearchSessionService>();
            
            return new DemoModeResearchService(realService, mockService, demoOptions);
        });
        
        // Register background service as singleton (it's a hosted service)
        services.AddSingleton<ResearchBackgroundService>();
        services.AddSingleton<IResearchBackgroundService>(provider => 
            provider.GetRequiredService<ResearchBackgroundService>());
        services.AddHostedService<ResearchBackgroundService>(provider => 
            provider.GetRequiredService<ResearchBackgroundService>());
        
        // Register other services normally
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<ISwotAnalysisService, SwotAnalysisService>();
        services.AddScoped<ICompetitiveAnalysisService, CompetitiveAnalysisService>();
        services.AddScoped<ICustomerSegmentationService, CustomerSegmentationService>();
        services.AddScoped<IResearchStrategyService, ResearchStrategyService>();
        services.AddScoped<IProgressNotificationService, SignalRProgressNotificationService>();
        services.AddScoped<IJwtService, JwtService>();
        
        // Register HttpClient for AI providers
        services.AddHttpClient();
        
        return services;
    }
}