using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class ParentsDataConected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentsDataId",
                table: "Patients",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MotherWork",
                table: "ParentsData",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MotherName",
                table: "ParentsData",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FatherWork",
                table: "ParentsData",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FatherName",
                table: "ParentsData",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "MotherWork",
                table: "ParentsData",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MotherName",
                table: "ParentsData",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FatherWork",
                table: "ParentsData",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FatherName",
                table: "ParentsData",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
