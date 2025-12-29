using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class CongErrorsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CongErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CongHypothyroidism = table.Column<bool>(type: "boolean", nullable: false),
                    ResultHipot = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    Phenylalanine = table.Column<bool>(type: "boolean", nullable: false),
                    ResultFenil = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    FQP = table.Column<bool>(type: "boolean", nullable: false),
                    ResultFQP = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    Other = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    Biotinidase = table.Column<bool>(type: "boolean", nullable: false),
                    ResultBiot = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    Galactosemia = table.Column<bool>(type: "boolean", nullable: false),
                    ResultGalac = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    OHP = table.Column<bool>(type: "boolean", nullable: false),
                    ResultOHP = table.Column<string>(type: "text", nullable: true, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CongErrors_Patients_Id",
                        column: x => x.Id,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CongErrors");
        }
    }
}
