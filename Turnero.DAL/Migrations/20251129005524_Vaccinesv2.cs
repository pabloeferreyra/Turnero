#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class Vaccinesv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VaccinesId",
                table: "Patients",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_VaccinesId",
                table: "Patients",
                column: "VaccinesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Vaccines_VaccinesId",
                table: "Patients",
                column: "VaccinesId",
                principalTable: "Vaccines",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Vaccines_VaccinesId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_VaccinesId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "VaccinesId",
                table: "Patients");
        }
    }
}
