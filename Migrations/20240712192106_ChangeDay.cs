using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Available_TimeTurns_TimeId",
                table: "Available");

            migrationBuilder.DropIndex(
                name: "IX_Available_TimeId",
                table: "Available");

            migrationBuilder.AlterColumn<string>(
                name: "Day",
                table: "Available",
                type: "text",
                nullable: false,
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Time",
                table: "Available",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TimeTurnId",
                table: "Available",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Available_TimeTurnId",
                table: "Available",
                column: "TimeTurnId");

            migrationBuilder.AddForeignKey(
                name: "FK_Available_TimeTurns_TimeTurnId",
                table: "Available",
                column: "TimeTurnId",
                principalTable: "TimeTurns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Available_TimeTurns_TimeTurnId",
                table: "Available");

            migrationBuilder.DropIndex(
                name: "IX_Available_TimeTurnId",
                table: "Available");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "Available");

            migrationBuilder.DropColumn(
                name: "TimeTurnId",
                table: "Available");

            migrationBuilder.AlterColumn<string>(
                name: "Day",
                table: "Available",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Available_TimeId",
                table: "Available",
                column: "TimeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Available_TimeTurns_TimeId",
                table: "Available",
                column: "TimeId",
                principalTable: "TimeTurns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
