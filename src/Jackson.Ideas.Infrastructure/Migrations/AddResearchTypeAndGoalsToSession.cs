using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jackson.Ideas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResearchTypeAndGoalsToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResearchType",
                table: "ResearchSessions",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Goals",
                table: "ResearchSessions",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResearchType",
                table: "ResearchSessions");

            migrationBuilder.DropColumn(
                name: "Goals",
                table: "ResearchSessions");
        }
    }
}