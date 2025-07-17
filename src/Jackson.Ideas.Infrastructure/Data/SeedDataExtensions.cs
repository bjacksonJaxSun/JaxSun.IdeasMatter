using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jackson.Ideas.Core.Entities;

namespace Jackson.Ideas.Infrastructure.Data;

public static class SeedDataExtensions
{
    public static async Task<IServiceProvider> SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<JacksonIdeasDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();

            var seeder = new DatabaseSeeder(context, userManager, roleManager, logger);
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();
            logger.LogError(ex, "An error occurred while seeding the database during application startup.");
            
            // In development, we might want to throw to see the error immediately
            // In production, we might want to continue startup and handle this gracefully
            throw;
        }

        return serviceProvider;
    }

    public static async Task<IServiceProvider> SeedDatabaseIfEmptyAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        
        var context = scope.ServiceProvider.GetRequiredService<JacksonIdeasDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();

        // Only seed if there are no users in the system
        var hasUsers = userManager.Users.Any();
        
        if (!hasUsers)
        {
            logger.LogInformation("No users found in database. Starting seeding process...");
            await serviceProvider.SeedDatabaseAsync();
        }
        else
        {
            logger.LogInformation("Users already exist in database. Skipping seeding process.");
        }

        return serviceProvider;
    }
}