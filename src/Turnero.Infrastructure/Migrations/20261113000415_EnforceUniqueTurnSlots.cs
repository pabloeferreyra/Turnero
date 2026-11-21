using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUniqueTurnSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""LOCK TABLE "Turns" IN SHARE ROW EXCLUSIVE MODE;""");

            migrationBuilder.DropIndex(
                name: "IX_Turns_MedicId",
                table: "Turns");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_MedicId_DateTurn_TimeId",
                table: "Turns",
                columns: new[] { "MedicId", "DateTurn", "TimeId" });

            migrationBuilder.Sql("""
                CREATE TABLE "TurnSlotReservations" (
                    "MedicId" uuid NOT NULL,
                    "DateTurn" date NOT NULL,
                    "TimeId" uuid NOT NULL,
                    "TurnCount" bigint NOT NULL CHECK ("TurnCount" >= 0),
                    PRIMARY KEY ("MedicId", "DateTurn", "TimeId")
                );

                INSERT INTO "TurnSlotReservations" ("MedicId", "DateTurn", "TimeId", "TurnCount")
                SELECT "MedicId", "DateTurn", "TimeId", COUNT(*)
                FROM "Turns"
                GROUP BY "MedicId", "DateTurn", "TimeId";

                CREATE FUNCTION "MaintainTurnSlotReservation"() RETURNS trigger
                LANGUAGE plpgsql AS $$
                DECLARE
                    remaining bigint;
                BEGIN
                    IF TG_OP = 'UPDATE' AND
                       (OLD."MedicId", OLD."DateTurn", OLD."TimeId") =
                       (NEW."MedicId", NEW."DateTurn", NEW."TimeId") THEN
                        RETURN NEW;
                    END IF;

                    IF TG_OP <> 'INSERT' THEN
                        UPDATE "TurnSlotReservations"
                        SET "TurnCount" = "TurnCount" - 1
                        WHERE "MedicId" = OLD."MedicId"
                          AND "DateTurn" = OLD."DateTurn"
                          AND "TimeId" = OLD."TimeId"
                          AND "TurnCount" > 0
                        RETURNING "TurnCount" INTO remaining;
                        IF NOT FOUND THEN
                            RAISE EXCEPTION 'No se encontró la reserva del horario.';
                        END IF;
                        IF remaining = 0 THEN
                            DELETE FROM "TurnSlotReservations"
                            WHERE "MedicId" = OLD."MedicId"
                              AND "DateTurn" = OLD."DateTurn"
                              AND "TimeId" = OLD."TimeId";
                        END IF;
                    END IF;

                    IF TG_OP <> 'DELETE' THEN
                        INSERT INTO "TurnSlotReservations" ("MedicId", "DateTurn", "TimeId", "TurnCount")
                        VALUES (NEW."MedicId", NEW."DateTurn", NEW."TimeId", 1)
                        ON CONFLICT DO NOTHING;
                        IF NOT FOUND THEN
                            RAISE EXCEPTION 'El horario ya fue reservado.'
                                USING ERRCODE = '23505',
                                      CONSTRAINT = 'IX_Turns_MedicId_DateTurn_TimeId';
                        END IF;
                        RETURN NEW;
                    END IF;
                    RETURN OLD;
                END;
                $$;

                CREATE TRIGGER "MaintainTurnSlotReservation"
                BEFORE INSERT OR DELETE OR UPDATE OF "MedicId", "DateTurn", "TimeId"
                ON "Turns" FOR EACH ROW EXECUTE FUNCTION "MaintainTurnSlotReservation"();

                CREATE FUNCTION "ClearTurnSlotReservations"() RETURNS trigger
                LANGUAGE plpgsql AS $$
                BEGIN
                    TRUNCATE TABLE "TurnSlotReservations";
                    RETURN NULL;
                END;
                $$;

                CREATE TRIGGER "ClearTurnSlotReservations"
                AFTER TRUNCATE ON "Turns"
                FOR EACH STATEMENT EXECUTE FUNCTION "ClearTurnSlotReservations"();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER "MaintainTurnSlotReservation" ON "Turns";
                DROP TRIGGER "ClearTurnSlotReservations" ON "Turns";
                DROP FUNCTION "MaintainTurnSlotReservation"();
                DROP FUNCTION "ClearTurnSlotReservations"();
                DROP TABLE "TurnSlotReservations";
                """);

            migrationBuilder.DropIndex(
                name: "IX_Turns_MedicId_DateTurn_TimeId",
                table: "Turns");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_MedicId",
                table: "Turns",
                column: "MedicId");
        }
    }
}
