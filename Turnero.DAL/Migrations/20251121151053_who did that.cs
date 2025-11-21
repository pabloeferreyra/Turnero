using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Migrations
{
    public partial class whodidthat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) limpiar datos según tu regla
            migrationBuilder.Sql(@"
                UPDATE ""Turns""
                SET ""Dni"" =
                    CASE
                        WHEN regexp_replace(""Dni""::text, '[^0-9]', '', 'g') = ''
                            THEN '11111111'
                        ELSE regexp_replace(""Dni""::text, '[^0-9]', '', 'g')
                    END;
            ");

            // 2) cambiar tipo usando SQL explícito con USING
            migrationBuilder.Sql(@"
                ALTER TABLE ""Turns""
                ALTER COLUMN ""Dni"" TYPE bigint
                USING (regexp_replace(""Dni""::text, '[^0-9]', '', 'g')::bigint);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Turns""
                ALTER COLUMN ""Dni"" TYPE character varying(10)
                USING (""Dni""::text);
            ");
        }
    }
}
