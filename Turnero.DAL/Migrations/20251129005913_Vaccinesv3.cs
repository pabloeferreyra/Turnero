#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class Vaccinesv3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_PerinatalBackground_PerinatalBackgroundId",
                table: "Patients");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Vaccines_VaccinesId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_PerinatalBackgroundId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_VaccinesId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PerinatalBackgroundId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "VaccinesId",
                table: "Patients");

            migrationBuilder.AddForeignKey(
                name: "FK_PerinatalBackground_Patients_Id",
                table: "PerinatalBackground",
                column: "Id",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vaccines_Patients_Id",
                table: "Vaccines",
                column: "Id",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PerinatalBackground_Patients_Id",
                table: "PerinatalBackground");

            migrationBuilder.DropForeignKey(
                name: "FK_Vaccines_Patients_Id",
                table: "Vaccines");

            migrationBuilder.AddColumn<Guid>(
                name: "PerinatalBackgroundId",
                table: "Patients",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VaccinesId",
                table: "Patients",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PerinatalBackgroundId",
                table: "Patients",
                column: "PerinatalBackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_VaccinesId",
                table: "Patients",
                column: "VaccinesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_PerinatalBackground_PerinatalBackgroundId",
                table: "Patients",
                column: "PerinatalBackgroundId",
                principalTable: "PerinatalBackground",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Vaccines_VaccinesId",
                table: "Patients",
                column: "VaccinesId",
                principalTable: "Vaccines",
                principalColumn: "Id");
        }
    }
}
