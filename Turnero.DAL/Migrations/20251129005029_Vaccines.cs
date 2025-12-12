#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class Vaccines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vaccines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Bcg = table.Column<DateOnly>(type: "date", nullable: false),
                    Sabin = table.Column<DateOnly>(type: "date", nullable: false),
                    HepB = table.Column<DateOnly>(type: "date", nullable: false),
                    Dpt = table.Column<DateOnly>(type: "date", nullable: false),
                    Hib = table.Column<DateOnly>(type: "date", nullable: false),
                    Prs = table.Column<DateOnly>(type: "date", nullable: false),
                    Dta = table.Column<DateOnly>(type: "date", nullable: false),
                    HepA = table.Column<DateOnly>(type: "date", nullable: false),
                    Pox = table.Column<DateOnly>(type: "date", nullable: false),
                    Pneumo = table.Column<DateOnly>(type: "date", nullable: false),
                    Meningo = table.Column<DateOnly>(type: "date", nullable: false),
                    Flu = table.Column<DateOnly>(type: "date", nullable: false),
                    Other = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vaccines", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vaccines");
        }
    }
}
