using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicAvailabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    DateTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    TimeFromId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeToId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicAvailabilities_Medics_MedicId",
                        column: x => x.MedicId,
                        principalTable: "Medics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicAvailabilities_TimeTurns_TimeFromId",
                        column: x => x.TimeFromId,
                        principalTable: "TimeTurns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicAvailabilities_TimeTurns_TimeToId",
                        column: x => x.TimeToId,
                        principalTable: "TimeTurns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicAvailabilities_MedicId",
                table: "MedicAvailabilities",
                column: "MedicId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicAvailabilities_TimeFromId",
                table: "MedicAvailabilities",
                column: "TimeFromId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicAvailabilities_TimeToId",
                table: "MedicAvailabilities",
                column: "TimeToId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicAvailabilities");
        }
    }
}
