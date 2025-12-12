#nullable disable

namespace Turnero.Migrations
{
    public partial class whodidthat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Turns""
                SET ""Dni"" =
                    CASE
                        WHEN regexp_replace(""Dni""::text, '[^0-9]', '', 'g') = ''
                            THEN '11111111'
                        ELSE regexp_replace(""Dni""::text, '[^0-9]', '', 'g')
                    END;
            ");

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
