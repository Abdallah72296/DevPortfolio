using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPortfolio.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "UserProfiles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "SocialLinks",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Skills",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "SkillCategories",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Projects",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "PortfolioSiteSettings",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "PortfolioSections",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "PortfolioAnalyticsEvents",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Experiences",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Educations",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "ContactMessages",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Certifications",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "PortfolioSections",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "OwnerUserId",
                value: "");

            migrationBuilder.UpdateData(
                table: "PortfolioSections",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "OwnerUserId",
                value: "");

            migrationBuilder.UpdateData(
                table: "PortfolioSections",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "OwnerUserId",
                value: "");

            migrationBuilder.UpdateData(
                table: "PortfolioSections",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "OwnerUserId",
                value: "");

            migrationBuilder.UpdateData(
                table: "PortfolioSections",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                column: "OwnerUserId",
                value: "");

            migrationBuilder.UpdateData(
                table: "PortfolioSections",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                column: "OwnerUserId",
                value: "");

            migrationBuilder.UpdateData(
                table: "PortfolioSections",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                column: "OwnerUserId",
                value: "");

            migrationBuilder.UpdateData(
                table: "PortfolioSections",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                column: "OwnerUserId",
                value: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_OwnerUserId",
                table: "UserProfiles",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialLinks_OwnerUserId",
                table: "SocialLinks",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_OwnerUserId",
                table: "Skills",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillCategories_OwnerUserId",
                table: "SkillCategories",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OwnerUserId",
                table: "Projects",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSiteSettings_OwnerUserId",
                table: "PortfolioSiteSettings",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSections_OwnerUserId",
                table: "PortfolioSections",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioAnalyticsEvents_OwnerUserId",
                table: "PortfolioAnalyticsEvents",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_OwnerUserId",
                table: "Experiences",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Educations_OwnerUserId",
                table: "Educations",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_OwnerUserId",
                table: "ContactMessages",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_OwnerUserId",
                table: "Certifications",
                column: "OwnerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_OwnerUserId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_SocialLinks_OwnerUserId",
                table: "SocialLinks");

            migrationBuilder.DropIndex(
                name: "IX_Skills_OwnerUserId",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_SkillCategories_OwnerUserId",
                table: "SkillCategories");

            migrationBuilder.DropIndex(
                name: "IX_Projects_OwnerUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioSiteSettings_OwnerUserId",
                table: "PortfolioSiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioSections_OwnerUserId",
                table: "PortfolioSections");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioAnalyticsEvents_OwnerUserId",
                table: "PortfolioAnalyticsEvents");

            migrationBuilder.DropIndex(
                name: "IX_Experiences_OwnerUserId",
                table: "Experiences");

            migrationBuilder.DropIndex(
                name: "IX_Educations_OwnerUserId",
                table: "Educations");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_OwnerUserId",
                table: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_Certifications_OwnerUserId",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "SocialLinks");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "SkillCategories");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "PortfolioSiteSettings");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "PortfolioSections");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "PortfolioAnalyticsEvents");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Educations");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Certifications");
        }
    }
}
