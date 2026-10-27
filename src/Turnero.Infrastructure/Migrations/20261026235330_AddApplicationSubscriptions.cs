using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ExternalReference = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MercadoPagoId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyId = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CheckoutUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSubscriptions_ExternalReference",
                table: "ApplicationSubscriptions",
                column: "ExternalReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSubscriptions_MercadoPagoId",
                table: "ApplicationSubscriptions",
                column: "MercadoPagoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSubscriptions_UserId",
                table: "ApplicationSubscriptions",
                column: "UserId",
                unique: true,
                filter: "\"Status\" IN ('creating', 'creation_unknown', 'pending', 'authorized', 'paused')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationSubscriptions");
        }
    }
}
