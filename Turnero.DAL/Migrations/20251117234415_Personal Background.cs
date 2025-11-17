using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class PersonalBackground : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalBackground",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Asthma = table.Column<bool>(type: "boolean", nullable: false),
                    Allergy = table.Column<bool>(type: "boolean", nullable: false),
                    Pulmonologist = table.Column<bool>(type: "boolean", nullable: false),
                    Pneumonia = table.Column<bool>(type: "boolean", nullable: false),
                    Mumps = table.Column<bool>(type: "boolean", nullable: false),
                    Psicologicals = table.Column<bool>(type: "boolean", nullable: false),
                    Accidents = table.Column<bool>(type: "boolean", nullable: false),
                    HematOnc = table.Column<bool>(type: "boolean", nullable: false),
                    Rubella = table.Column<bool>(type: "boolean", nullable: false),
                    Otitis = table.Column<bool>(type: "boolean", nullable: false),
                    Measles = table.Column<bool>(type: "boolean", nullable: false),
                    Chickenpox = table.Column<bool>(type: "boolean", nullable: false),
                    UrinaryInfections = table.Column<bool>(type: "boolean", nullable: false),
                    Surgeries = table.Column<bool>(type: "boolean", nullable: false),
                    Diabetes = table.Column<bool>(type: "boolean", nullable: false),
                    Digestive = table.Column<bool>(type: "boolean", nullable: false),
                    Other = table.Column<string>(type: "text", nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalBackground", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalBackground_Patients_Id",
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
                name: "PersonalBackground");
        }
    }
}
