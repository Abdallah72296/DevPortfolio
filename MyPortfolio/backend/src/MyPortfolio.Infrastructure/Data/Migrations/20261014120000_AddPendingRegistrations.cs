using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MyPortfolio.Infrastructure.Data;

#nullable disable

namespace MyPortfolio.Infrastructure.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261014120000_AddPendingRegistrations")]
public sealed class AddPendingRegistrations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PendingRegistrations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                NormalizedUsername = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                OtpHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                OtpExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                OtpAttempts = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PendingRegistrations", x => x.Id));

        migrationBuilder.CreateIndex("IX_PendingRegistrations_NormalizedEmail", "PendingRegistrations", "NormalizedEmail", unique: true);
        migrationBuilder.CreateIndex("IX_PendingRegistrations_NormalizedUsername", "PendingRegistrations", "NormalizedUsername", unique: true);
        migrationBuilder.CreateIndex("IX_PendingRegistrations_OtpExpiresAt", "PendingRegistrations", "OtpExpiresAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "PendingRegistrations");
}
