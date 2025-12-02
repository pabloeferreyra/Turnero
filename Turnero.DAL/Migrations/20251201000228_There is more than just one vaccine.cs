#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class Thereismorethanjustonevaccine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vaccines_Patients_Id",
                table: "Vaccines");

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "Vaccines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Vaccines_PatientId",
                table: "Vaccines",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vaccines_Patients_PatientId",
                table: "Vaccines",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vaccines_Patients_PatientId",
                table: "Vaccines");

            migrationBuilder.DropIndex(
                name: "IX_Vaccines_PatientId",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "Vaccines");

            migrationBuilder.AddForeignKey(
                name: "FK_Vaccines_Patients_Id",
                table: "Vaccines",
                column: "Id",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
