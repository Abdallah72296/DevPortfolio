using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPortfolio.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagesJson",
                table: "Projects",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PortfolioSiteSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThemeKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CustomDomain = table.Column<string>(type: "nvarchar(253)", maxLength: 253, nullable: false),
                    SeoTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SeoDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SeoKeywords = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioSiteSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSiteSettings_CustomDomain",
                table: "PortfolioSiteSettings",
                column: "CustomDomain",
                unique: true,
                filter: "[CustomDomain] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioSiteSettings");

            migrationBuilder.DropColumn(
                name: "ImagesJson",
                table: "Projects");
        }
    }
}
