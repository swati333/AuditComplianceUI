using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finding.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActionPlanReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActionPlanReferences",
                columns: table => new
                {
                    FindingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstActionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionPlanReferences", x => x.FindingId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionPlanReferences");
        }
    }
}
