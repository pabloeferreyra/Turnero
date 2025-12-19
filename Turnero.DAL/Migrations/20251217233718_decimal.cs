using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class @decimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "GrowthCharts",
                type: "numeric(13,3)",
                precision: 13,
                scale: 3,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "Height",
                table: "GrowthCharts",
                type: "numeric(13,3)",
                precision: 13,
                scale: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "HeadCircumference",
                table: "GrowthCharts",
                type: "numeric(13,3)",
                precision: 13,
                scale: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "Bmi",
                table: "GrowthCharts",
                type: "numeric(13,3)",
                precision: 13,
                scale: 3,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "Age",
                table: "GrowthCharts",
                type: "numeric(6,3)",
                precision: 6,
                scale: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Weight",
                table: "GrowthCharts",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(13,3)",
                oldPrecision: 13,
                oldScale: 3);

            migrationBuilder.AlterColumn<int>(
                name: "Height",
                table: "GrowthCharts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(13,3)",
                oldPrecision: 13,
                oldScale: 3);

            migrationBuilder.AlterColumn<int>(
                name: "HeadCircumference",
                table: "GrowthCharts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(13,3)",
                oldPrecision: 13,
                oldScale: 3);

            migrationBuilder.AlterColumn<double>(
                name: "Bmi",
                table: "GrowthCharts",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(13,3)",
                oldPrecision: 13,
                oldScale: 3);

            migrationBuilder.AlterColumn<int>(
                name: "Age",
                table: "GrowthCharts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(6,3)",
                oldPrecision: 6,
                oldScale: 3);
        }
    }
}
