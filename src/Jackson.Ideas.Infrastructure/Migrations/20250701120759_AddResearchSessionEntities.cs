using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jackson.Ideas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResearchSessionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResearchSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ResearchApproach = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EstimatedDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgressPercentage = table.Column<double>(type: "REAL", nullable: false),
                    CurrentPhase = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AnalysisConfidence = table.Column<double>(type: "REAL", nullable: true),
                    AnalysisCompleteness = table.Column<double>(type: "REAL", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    NextSteps = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResearchInsights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ResearchSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Phase = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "REAL", precision: 3, scale: 2, nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}"),
                    InsightType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchInsights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchInsights_ResearchSessions_ResearchSessionId",
                        column: x => x.ResearchSessionId,
                        principalTable: "ResearchSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResearchOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ResearchSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Approach = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TargetCustomerSegment = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ValueProposition = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    GoToMarketStrategy = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OverallScore = table.Column<double>(type: "REAL", precision: 3, scale: 1, nullable: false),
                    TimelineToMarketMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    TimelineToProfitabilityMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessProbabilityPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedInvestmentUsd = table.Column<decimal>(type: "TEXT", precision: 15, scale: 2, nullable: false),
                    IsRecommended = table.Column<bool>(type: "INTEGER", nullable: false),
                    BusinessModel = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}"),
                    RiskFactors = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]"),
                    MitigationStrategies = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]"),
                    SuccessMetrics = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]"),
                    SwotAnalysis = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}"),
                    CompetitivePositioning = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchOptions_ResearchSessions_ResearchSessionId",
                        column: x => x.ResearchSessionId,
                        principalTable: "ResearchSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchInsights_Phase",
                table: "ResearchInsights",
                column: "Phase");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchInsights_ResearchSessionId",
                table: "ResearchInsights",
                column: "ResearchSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchOptions_IsRecommended",
                table: "ResearchOptions",
                column: "IsRecommended");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchOptions_OverallScore",
                table: "ResearchOptions",
                column: "OverallScore");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchOptions_ResearchSessionId",
                table: "ResearchOptions",
                column: "ResearchSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchSessions_CreatedAt",
                table: "ResearchSessions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchSessions_Status",
                table: "ResearchSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchSessions_UserId",
                table: "ResearchSessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResearchInsights");

            migrationBuilder.DropTable(
                name: "ResearchOptions");

            migrationBuilder.DropTable(
                name: "ResearchSessions");
        }
    }
}
