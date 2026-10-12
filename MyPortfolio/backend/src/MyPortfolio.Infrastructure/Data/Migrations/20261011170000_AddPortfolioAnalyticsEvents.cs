using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPortfolio.Infrastructure.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261011170000_AddPortfolioAnalyticsEvents")]
public partial class AddPortfolioAnalyticsEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PortfolioAnalyticsEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PortfolioAnalyticsEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_PortfolioAnalyticsEvents_UserProfiles_ProfileId",
                    column: x => x.ProfileId,
                    principalTable: "UserProfiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PortfolioAnalyticsEvents_ProfileId_EventType_CreatedAt",
            table: "PortfolioAnalyticsEvents",
            columns: new[] { "ProfileId", "EventType", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PortfolioAnalyticsEvents");
    }
}
