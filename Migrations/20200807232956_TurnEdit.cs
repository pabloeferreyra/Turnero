using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Turnero.Migrations
{
    public partial class TurnEdit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turns_TimeTurns_TimeId",
                table: "Turns");

            migrationBuilder.AlterColumn<Guid>(
                name: "TimeId",
                table: "Turns",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_TimeTurns_TimeId",
                table: "Turns",
                column: "TimeId",
                principalTable: "TimeTurns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turns_TimeTurns_TimeId",
                table: "Turns");

            migrationBuilder.AlterColumn<Guid>(
                name: "TimeId",
                table: "Turns",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_TimeTurns_TimeId",
                table: "Turns",
                column: "TimeId",
                principalTable: "TimeTurns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
