using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectricityPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxGroupIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TaxGroups",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TaxGroups");
        }
    }
}
