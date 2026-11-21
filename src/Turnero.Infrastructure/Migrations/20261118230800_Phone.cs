using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Phone",
                table: "Turns",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Turns");
        }
    }
}
