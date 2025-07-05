using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jackson.Ideas.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<JacksonIdeasDbContext>
{
    public JacksonIdeasDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<JacksonIdeasDbContext>();
        
        // Use SQLite for design-time migrations
        optionsBuilder.UseSqlite("Data Source=jackson_ideas_design.db");
        
        return new JacksonIdeasDbContext(optionsBuilder.Options);
    }
}