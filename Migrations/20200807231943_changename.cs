using Microsoft.EntityFrameworkCore.Migrations;

namespace Turnero.Migrations
{
    public partial class changename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turns_TimeTurnViewModel_TimeId",
                table: "Turns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeTurnViewModel",
                table: "TimeTurnViewModel");

            migrationBuilder.RenameTable(
                name: "TimeTurnViewModel",
                newName: "TimeTurns");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeTurns",
                table: "TimeTurns",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_TimeTurns_TimeId",
                table: "Turns",
                column: "TimeId",
                principalTable: "TimeTurns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turns_TimeTurns_TimeId",
                table: "Turns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeTurns",
                table: "TimeTurns");

            migrationBuilder.RenameTable(
                name: "TimeTurns",
                newName: "TimeTurnViewModel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeTurnViewModel",
                table: "TimeTurnViewModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_TimeTurnViewModel_TimeId",
                table: "Turns",
                column: "TimeId",
                principalTable: "TimeTurnViewModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
