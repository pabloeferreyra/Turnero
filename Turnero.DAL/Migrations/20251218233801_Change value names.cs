using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class Changevaluenames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResultHipot",
                table: "CongErrors",
                newName: "ResultPhenylalanine");

            migrationBuilder.RenameColumn(
                name: "ResultGalac",
                table: "CongErrors",
                newName: "ResultHypothyroidism");

            migrationBuilder.RenameColumn(
                name: "ResultFenil",
                table: "CongErrors",
                newName: "ResultGalactosemia");

            migrationBuilder.RenameColumn(
                name: "ResultBiot",
                table: "CongErrors",
                newName: "ResultBiotinidase");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResultPhenylalanine",
                table: "CongErrors",
                newName: "ResultHipot");

            migrationBuilder.RenameColumn(
                name: "ResultHypothyroidism",
                table: "CongErrors",
                newName: "ResultGalac");

            migrationBuilder.RenameColumn(
                name: "ResultGalactosemia",
                table: "CongErrors",
                newName: "ResultFenil");

            migrationBuilder.RenameColumn(
                name: "ResultBiotinidase",
                table: "CongErrors",
                newName: "ResultBiot");
        }
    }
}
