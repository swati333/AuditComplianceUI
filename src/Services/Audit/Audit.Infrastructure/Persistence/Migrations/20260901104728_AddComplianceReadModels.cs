using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComplianceReadModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FindingReferences",
                columns: table => new
                {
                    FindingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FindingReferences", x => x.FindingId);
                });

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => new { x.Id, x.ConsumerName });
                });

            migrationBuilder.CreateTable(
                name: "OpenCriticalFindings",
                columns: table => new
                {
                    FindingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DetectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenCriticalFindings", x => x.FindingId);
                });

            migrationBuilder.CreateTable(
                name: "OpenRequiredActions",
                columns: table => new
                {
                    ActionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FindingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenRequiredActions", x => x.ActionPlanId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FindingReferences_AuditId",
                table: "FindingReferences",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenCriticalFindings_AuditId",
                table: "OpenCriticalFindings",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenRequiredActions_AuditId",
                table: "OpenRequiredActions",
                column: "AuditId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FindingReferences");

            migrationBuilder.DropTable(
                name: "InboxMessages");

            migrationBuilder.DropTable(
                name: "OpenCriticalFindings");

            migrationBuilder.DropTable(
                name: "OpenRequiredActions");
        }
    }
}
