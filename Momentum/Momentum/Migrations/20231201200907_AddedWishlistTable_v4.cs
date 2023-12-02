using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Momentum.Migrations
{
    public partial class AddedWishlistTable_v4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Wishlists");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Wishlists",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
