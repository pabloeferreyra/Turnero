using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class ParentsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParentsData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FatherName = table.Column<string>(type: "text", nullable: false),
                    FatherBirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FatherBloodType = table.Column<int>(type: "integer", nullable: false),
                    FatherWork = table.Column<string>(type: "text", nullable: false),
                    MotherName = table.Column<string>(type: "text", nullable: false),
                    MotherBirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MotherBloodType = table.Column<int>(type: "integer", nullable: false),
                    MotherWork = table.Column<string>(type: "text", nullable: false),
                    BrothersCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentsData", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParentsData");
        }
    }
}
