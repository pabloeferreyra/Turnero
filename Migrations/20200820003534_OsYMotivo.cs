using Microsoft.EntityFrameworkCore.Migrations;

namespace Turnero.Migrations
{
    public partial class OsYMotivo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Turns",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialWork",
                table: "Turns",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "SocialWork",
                table: "Turns");
        }
    }
}
