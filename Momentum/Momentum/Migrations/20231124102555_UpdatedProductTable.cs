using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Momentum.Migrations
{
    public partial class UpdatedProductTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EcoTax",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "Seria",
                table: "Products",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Seria",
                table: "Products");

            migrationBuilder.AddColumn<decimal>(
                name: "EcoTax",
                table: "Products",
                type: "money",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
