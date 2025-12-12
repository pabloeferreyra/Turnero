#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class TableChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bcg",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Dpt",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Dta",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Flu",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "HepA",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "HepB",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Hib",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Meningo",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Other",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Pneumo",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Pox",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "Prs",
                table: "Vaccines");

            migrationBuilder.RenameColumn(
                name: "Sabin",
                table: "Vaccines",
                newName: "DateApplied");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateApplied",
                table: "Vaccines",
                newName: "Sabin");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Bcg",
                table: "Vaccines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Dpt",
                table: "Vaccines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Dta",
                table: "Vaccines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Flu",
                table: "Vaccines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "HepA",
                table: "Vaccines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "HepB",
                table: "Vaccines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Hib",
                table: "Vaccines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Meningo",
                table: "Vaccines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Other",
                table: "Vaccines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Pneumo",
                table: "Vaccines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Pox",
                table: "Vaccines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Prs",
                table: "Vaccines",
                type: "date",
                nullable: true);
        }
    }
}
