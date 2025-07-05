using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jackson.Ideas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    HashedPassword = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Picture = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Permissions = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    AuthProvider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "local"),
                    LastLogin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIProviderConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    EncryptedApiKey = table.Column<string>(type: "TEXT", nullable: false),
                    Config = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalTokens = table.Column<long>(type: "INTEGER", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RateLimitRpm = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentRpmUsage = table.Column<int>(type: "INTEGER", nullable: false),
                    CostPerToken = table.Column<decimal>(type: "TEXT", precision: 10, scale: 8, nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIProviderConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIProviderConfigs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Researches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ResearchType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "comprehensive"),
                    AIProvider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IncludeMarketAnalysis = table.Column<bool>(type: "INTEGER", nullable: false),
                    IncludeSwot = table.Column<bool>(type: "INTEGER", nullable: false),
                    IncludeBusinessPlan = table.Column<bool>(type: "INTEGER", nullable: false),
                    MarketAnalysis = table.Column<string>(type: "TEXT", nullable: true),
                    SwotAnalysis = table.Column<string>(type: "TEXT", nullable: true),
                    BusinessPlan = table.Column<string>(type: "TEXT", nullable: true),
                    Competitors = table.Column<string>(type: "TEXT", nullable: true),
                    ProviderInsights = table.Column<string>(type: "TEXT", nullable: true),
                    ProgressPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentStep = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TokensUsed = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Researches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Researches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MarketSize = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    GrowthRate = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TargetAudience = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CompetitiveLandscape = table.Column<string>(type: "TEXT", nullable: true),
                    KeyTrends = table.Column<string>(type: "TEXT", nullable: true),
                    CustomerSegments = table.Column<string>(type: "TEXT", nullable: true),
                    RegulatoryEnvironment = table.Column<string>(type: "TEXT", nullable: true),
                    RevenueModels = table.Column<string>(type: "TEXT", nullable: true),
                    EntryBarriers = table.Column<string>(type: "TEXT", nullable: true),
                    GeographicScope = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Industry = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ResearchId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketAnalyses_Researches_ResearchId",
                        column: x => x.ResearchId,
                        principalTable: "Researches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIProviderConfigs_IsActive",
                table: "AIProviderConfigs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AIProviderConfigs_Type",
                table: "AIProviderConfigs",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AIProviderConfigs_UserId",
                table: "AIProviderConfigs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketAnalyses_ResearchId",
                table: "MarketAnalyses",
                column: "ResearchId");

            migrationBuilder.CreateIndex(
                name: "IX_Researches_CreatedAt",
                table: "Researches",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Researches_Status",
                table: "Researches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Researches_UserId",
                table: "Researches",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIProviderConfigs");

            migrationBuilder.DropTable(
                name: "MarketAnalyses");

            migrationBuilder.DropTable(
                name: "Researches");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
