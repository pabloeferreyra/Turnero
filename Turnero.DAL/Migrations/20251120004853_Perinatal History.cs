using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class PerinatalHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Visits",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Diagnosis",
                table: "Visits",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "UrinaryInfections",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Surgeries",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Rubella",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Pulmonologist",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Psicologicals",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Pneumonia",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Otitis",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Other",
                table: "PersonalBackground",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "Mumps",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Measles",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "HematOnc",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Digestive",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Diabetes",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Chickenpox",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Asthma",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Allergy",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "Accidents",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<Guid>(
                name: "PerinatalBackgroundId",
                table: "Patients",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MotherWork",
                table: "ParentsData",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MotherName",
                table: "ParentsData",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "MotherBirthDate",
                table: "ParentsData",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "FatherWork",
                table: "ParentsData",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FatherName",
                table: "ParentsData",
                type: "text",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "FatherBirthDate",
                table: "ParentsData",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<int>(
                name: "BrothersCount",
                table: "ParentsData",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Begin",
                table: "Allergies",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.CreateTable(
                name: "PerinatalBackground",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Feat = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Delivery = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Cesarean = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Abort = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Weight = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Height = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CefPer = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Apgar1 = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Apgar5 = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GestAge = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Pathologies = table.Column<string>(type: "text", nullable: true),
                    CongErrors = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerinatalBackground", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PerinatalBackgroundId",
                table: "Patients",
                column: "PerinatalBackgroundId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_PerinatalBackground_PerinatalBackgroundId",
                table: "Patients",
                column: "PerinatalBackgroundId",
                principalTable: "PerinatalBackground",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_PerinatalBackground_PerinatalBackgroundId",
                table: "Patients");

            migrationBuilder.DropTable(
                name: "PerinatalBackground");

            migrationBuilder.DropIndex(
                name: "IX_Patients_PerinatalBackgroundId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PerinatalBackgroundId",
                table: "Patients");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Visits",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Diagnosis",
                table: "Visits",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "UrinaryInfections",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Surgeries",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Rubella",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Pulmonologist",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Psicologicals",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Pneumonia",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Otitis",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Other",
                table: "PersonalBackground",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "Mumps",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Measles",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "HematOnc",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Digestive",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Diabetes",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Chickenpox",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Asthma",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Allergy",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Accidents",
                table: "PersonalBackground",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "MotherWork",
                table: "ParentsData",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "MotherName",
                table: "ParentsData",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "MotherBirthDate",
                table: "ParentsData",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AlterColumn<string>(
                name: "FatherWork",
                table: "ParentsData",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "FatherName",
                table: "ParentsData",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "FatherBirthDate",
                table: "ParentsData",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AlterColumn<int>(
                name: "BrothersCount",
                table: "ParentsData",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Begin",
                table: "Allergies",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
