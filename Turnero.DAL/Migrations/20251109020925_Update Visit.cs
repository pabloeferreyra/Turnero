using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiagDescription",
                table: "Visits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvolutionNotes",
                table: "Visits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabResults",
                table: "Visits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observations",
                table: "Visits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherStudies",
                table: "Visits",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiagDescription",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "EvolutionNotes",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "LabResults",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Observations",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "OtherStudies",
                table: "Visits");
        }
    }
}
