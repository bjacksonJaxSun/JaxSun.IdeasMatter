using Jackson.Ideas.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jackson.Ideas.Infrastructure.Data;

public class JacksonIdeasDbContext : IdentityDbContext<ApplicationUser>
{
    public JacksonIdeasDbContext(DbContextOptions<JacksonIdeasDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<Research> Researches => Set<Research>();
    public DbSet<MarketAnalysis> MarketAnalyses => Set<MarketAnalysis>();
    public DbSet<AIProviderConfig> AIProviderConfigs => Set<AIProviderConfig>();
    public DbSet<ResearchSession> ResearchSessions => Set<ResearchSession>();
    public DbSet<ResearchInsight> ResearchInsights => Set<ResearchInsight>();
    public DbSet<ResearchOption> ResearchOptions => Set<ResearchOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Picture).HasMaxLength(500);
            entity.Property(e => e.TenantId).HasMaxLength(100);
            entity.Property(e => e.AuthProvider).HasMaxLength(50).HasDefaultValue("local");
            entity.Property(e => e.Role).HasConversion<string>();
            entity.Property(e => e.Permissions).HasDefaultValue("[]");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure relationships
            entity.HasMany(u => u.Researches)
                  .WithOne(r => r.User)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(u => u.AIProviderConfigs)
                  .WithOne(a => a.User)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasMany(u => u.ResearchSessions)
                  .WithOne(rs => rs.User)
                  .HasForeignKey(rs => rs.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Research configuration
        modelBuilder.Entity<Research>(entity =>
        {
            entity.ToTable("Researches");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.ResearchType).HasMaxLength(50).HasDefaultValue("comprehensive");
            entity.Property(e => e.AIProvider).HasMaxLength(50);
            entity.Property(e => e.CurrentStep).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.EstimatedCost).HasPrecision(10, 4);
            
            // Configure JSON columns
            entity.Property(e => e.MarketAnalysisJson).HasColumnName("MarketAnalysis");
            entity.Property(e => e.SwotAnalysisJson).HasColumnName("SwotAnalysis");
            entity.Property(e => e.BusinessPlanJson).HasColumnName("BusinessPlan");
            entity.Property(e => e.CompetitorsJson).HasColumnName("Competitors");
            entity.Property(e => e.ProviderInsightsJson).HasColumnName("ProviderInsights");
            
            // Ignore computed properties
            entity.Ignore(e => e.MarketAnalysis);
            entity.Ignore(e => e.SwotAnalysis);
            entity.Ignore(e => e.BusinessPlan);
            entity.Ignore(e => e.Competitors);
            entity.Ignore(e => e.ProviderInsights);
            
            // Configure relationships
            entity.HasOne<ApplicationUser>(r => r.User)
                  .WithMany(u => u.Researches)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(r => r.MarketAnalyses)
                  .WithOne(m => m.Research)
                  .HasForeignKey(m => m.ResearchId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // MarketAnalysis configuration
        modelBuilder.Entity<MarketAnalysis>(entity =>
        {
            entity.ToTable("MarketAnalyses");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ResearchId);
            
            entity.Property(e => e.MarketSize).HasMaxLength(255);
            entity.Property(e => e.GrowthRate).HasMaxLength(100);
            entity.Property(e => e.TargetAudience).HasMaxLength(500);
            entity.Property(e => e.GeographicScope).HasMaxLength(100);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure JSON columns
            entity.Property(e => e.CompetitiveLandscapeJson).HasColumnName("CompetitiveLandscape");
            entity.Property(e => e.KeyTrendsJson).HasColumnName("KeyTrends");
            entity.Property(e => e.CustomerSegmentsJson).HasColumnName("CustomerSegments");
            entity.Property(e => e.RegulatoryEnvironmentJson).HasColumnName("RegulatoryEnvironment");
            entity.Property(e => e.RevenueModelsJson).HasColumnName("RevenueModels");
            entity.Property(e => e.EntryBarriersJson).HasColumnName("EntryBarriers");
            
            // Ignore computed properties
            entity.Ignore(e => e.CompetitiveLandscape);
            entity.Ignore(e => e.KeyTrends);
            entity.Ignore(e => e.CustomerSegments);
            entity.Ignore(e => e.RegulatoryEnvironment);
            entity.Ignore(e => e.RevenueModels);
            entity.Ignore(e => e.EntryBarriers);
            
            // Configure relationships
            entity.HasOne(m => m.Research)
                  .WithMany(r => r.MarketAnalyses)
                  .HasForeignKey(m => m.ResearchId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // AIProviderConfig configuration
        modelBuilder.Entity<AIProviderConfig>(entity =>
        {
            entity.ToTable("AIProviderConfigs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.EncryptedApiKey).IsRequired();
            entity.Property(e => e.ConfigJson).HasColumnName("Config").HasDefaultValue("{}");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CostPerToken).HasPrecision(10, 8);
            
            // Ignore computed properties
            entity.Ignore(e => e.Config);
            
            // Configure relationships
            entity.HasOne<ApplicationUser>(a => a.User)
                  .WithMany(u => u.AIProviderConfigs)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ResearchSession configuration
        modelBuilder.Entity<ResearchSession>(entity =>
        {
            entity.ToTable("ResearchSessions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.ResearchApproach).HasMaxLength(50);
            entity.Property(e => e.CurrentPhase).HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.NextSteps).HasDefaultValue("[]");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure relationships
            entity.HasOne<ApplicationUser>(rs => rs.User)
                  .WithMany(u => u.ResearchSessions)
                  .HasForeignKey(rs => rs.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(rs => rs.ResearchInsights)
                  .WithOne(ri => ri.ResearchSession)
                  .HasForeignKey(ri => ri.ResearchSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(rs => rs.ResearchOptions)
                  .WithOne(ro => ro.ResearchSession)
                  .HasForeignKey(ro => ro.ResearchSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ResearchInsight configuration
        modelBuilder.Entity<ResearchInsight>(entity =>
        {
            entity.ToTable("ResearchInsights");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ResearchSessionId);
            entity.HasIndex(e => e.Phase);
            
            entity.Property(e => e.Phase).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ConfidenceScore).HasPrecision(3, 2);
            entity.Property(e => e.Metadata).HasDefaultValue("{}");
            entity.Property(e => e.InsightType).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure relationships
            entity.HasOne(ri => ri.ResearchSession)
                  .WithMany(rs => rs.ResearchInsights)
                  .HasForeignKey(ri => ri.ResearchSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ResearchOption configuration
        modelBuilder.Entity<ResearchOption>(entity =>
        {
            entity.ToTable("ResearchOptions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ResearchSessionId);
            entity.HasIndex(e => e.OverallScore);
            entity.HasIndex(e => e.IsRecommended);
            
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Approach).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TargetCustomerSegment).HasMaxLength(500);
            entity.Property(e => e.ValueProposition).HasMaxLength(500);
            entity.Property(e => e.GoToMarketStrategy).HasMaxLength(500);
            entity.Property(e => e.OverallScore).HasPrecision(3, 1);
            entity.Property(e => e.EstimatedInvestmentUsd).HasPrecision(15, 2);
            entity.Property(e => e.BusinessModel).HasDefaultValue("{}");
            entity.Property(e => e.RiskFactors).HasDefaultValue("[]");
            entity.Property(e => e.MitigationStrategies).HasDefaultValue("[]");
            entity.Property(e => e.SuccessMetrics).HasDefaultValue("[]");
            entity.Property(e => e.SwotAnalysis).HasDefaultValue("{}");
            entity.Property(e => e.CompetitivePositioning).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure relationships
            entity.HasOne(ro => ro.ResearchSession)
                  .WithMany(rs => rs.ResearchOptions)
                  .HasForeignKey(ro => ro.ResearchSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }
    
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}