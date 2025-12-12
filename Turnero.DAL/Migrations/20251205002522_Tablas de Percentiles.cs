#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class TablasdePercentiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrowthCharts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: false),
                    WPerc = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    HPerc = table.Column<int>(type: "integer", nullable: false),
                    HeadCircumference = table.Column<int>(type: "integer", nullable: false),
                    HCPerc = table.Column<int>(type: "integer", nullable: false),
                    Bmi = table.Column<double>(type: "double precision", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrowthCharts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrowthCharts_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrowthCharts_PatientId",
                table: "GrowthCharts",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrowthCharts");
        }
    }
}
