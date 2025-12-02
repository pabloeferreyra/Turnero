#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class Wrongway : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_ParentsData_ParentsDataId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_ParentsDataId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ParentsDataId",
                table: "Patients");

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "ParentsData",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentsData_PatientId",
                table: "ParentsData",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentsData_Patients_PatientId",
                table: "ParentsData",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParentsData_Patients_PatientId",
                table: "ParentsData");

            migrationBuilder.DropIndex(
                name: "IX_ParentsData_PatientId",
                table: "ParentsData");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "ParentsData");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentsDataId",
                table: "Patients",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ParentsDataId",
                table: "Patients",
                column: "ParentsDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_ParentsData_ParentsDataId",
                table: "Patients",
                column: "ParentsDataId",
                principalTable: "ParentsData",
                principalColumn: "Id");
        }
    }
}
