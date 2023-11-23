using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Momentum.Migrations
{
    public partial class UpdatedBlogTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackImage",
                table: "Blogs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackImage",
                table: "Blogs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
