using Microsoft.EntityFrameworkCore.Migrations;

namespace WiredBrain.Identity.Data.Migrations
{
    public partial class addwiredinfoagain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoyaltyNumber",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoyaltyNumber",
                table: "AspNetUsers");
        }
    }
}
