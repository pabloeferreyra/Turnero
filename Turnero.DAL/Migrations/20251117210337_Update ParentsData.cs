using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class UpdateParentsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddForeignKey(
                name: "FK_ParentsData_Patients_Id",
                table: "ParentsData",
                column: "Id",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParentsData_Patients_Id",
                table: "ParentsData");

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
    }
}
