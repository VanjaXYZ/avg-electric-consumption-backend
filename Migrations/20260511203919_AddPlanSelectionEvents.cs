using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ElectricityPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanSelectionEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanSelectionEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Kwh = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxGroupId = table.Column<int>(type: "integer", nullable: false),
                    RecommendedPlanId = table.Column<int>(type: "integer", nullable: false),
                    RecommendedGrandTotal = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanSelectionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanSelectionEvents_Plans_RecommendedPlanId",
                        column: x => x.RecommendedPlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanSelectionEvents_TaxGroups_TaxGroupId",
                        column: x => x.TaxGroupId,
                        principalTable: "TaxGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanSelectionEvents_CreatedAtUtc",
                table: "PlanSelectionEvents",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PlanSelectionEvents_RecommendedPlanId",
                table: "PlanSelectionEvents",
                column: "RecommendedPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanSelectionEvents_TaxGroupId",
                table: "PlanSelectionEvents",
                column: "TaxGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanSelectionEvents");
        }
    }
}
