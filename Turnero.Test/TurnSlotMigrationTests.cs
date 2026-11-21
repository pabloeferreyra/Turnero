using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Turnero.Domain.Entities;
using Turnero.Infrastructure.Data;
using Xunit;

namespace Turnero.Test;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TURNERO_TEST_POSTGRES")))
        {
            Skip = "Requires TURNERO_TEST_POSTGRES pointing to a dedicated PostgreSQL test database.";
        }
    }
}

public class TurnSlotMigrationTests
{
    [PostgreSqlFact]
    public async Task PreservesExistingDuplicatesAndPreventsNewConflicts()
    {
        var connectionString = Environment.GetEnvironmentVariable("TURNERO_TEST_POSTGRES")!;
        var schema = "turn_slot_test_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connectionString);
        await admin.OpenAsync();
        await Execute(admin, $"""CREATE SCHEMA "{schema}";""");

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString) { SearchPath = schema };
            await using var connection = new NpgsqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
            await Execute(connection, """
                CREATE TABLE "Turns" (
                    "Id" uuid PRIMARY KEY, "MedicId" uuid NOT NULL,
                    "DateTurn" date NOT NULL, "TimeId" uuid NOT NULL
                );
                CREATE TABLE "Medics" ("Id" uuid PRIMARY KEY);
                CREATE TABLE "TimeTurns" ("Id" uuid PRIMARY KEY);
                CREATE INDEX "IX_Turns_MedicId" ON "Turns" ("MedicId");
                CREATE TABLE "__EFMigrationsHistory" (
                    "MigrationId" varchar(150) PRIMARY KEY, "ProductVersion" varchar(32) NOT NULL
                );
                INSERT INTO "__EFMigrationsHistory" VALUES
                    ('20260728001707_InitialCreate', '10.0.14'),
                    ('20261026235330_AddApplicationSubscriptions', '10.0.14');
                """);

            var medic = Guid.NewGuid();
            var occupiedTime = Guid.NewGuid();
            var otherTime = Guid.NewGuid();
            var duplicateTime = Guid.NewGuid();
            var first = Guid.NewGuid();
            var second = Guid.NewGuid();
            var other = Guid.NewGuid();
            var duplicateFirst = Guid.NewGuid();
            var duplicateSecond = Guid.NewGuid();
            await Execute(connection, Insert(first, medic, occupiedTime));
            await Execute(connection, Insert(second, medic, occupiedTime));
            await Execute(connection, Insert(other, medic, otherTime));
            await Execute(connection, Insert(duplicateFirst, medic, duplicateTime));
            await Execute(connection, Insert(duplicateSecond, medic, duplicateTime));

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.ConnectionString));
            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            await using var provider = services.BuildServiceProvider();
            await using var scope = provider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var migrator = context.GetService<IMigrator>();
            const string previous = "20261026235330_AddApplicationSubscriptions";
            Assert.False(context.Database.HasPendingModelChanges());
            await migrator.MigrateAsync();
            var snapshot = context.GetService<IMigrationsAssembly>().ModelSnapshot!.Model;
            var index = snapshot.FindEntityType(typeof(Turn))!.GetIndexes()
                .Single(item => item.Properties.Select(property => property.Name)
                    .SequenceEqual([nameof(Turn.MedicId), nameof(Turn.DateTurn), nameof(Turn.TimeId)]));
            Assert.False(index.IsUnique);

            Assert.Equal(5L, await Scalar(connection, """SELECT COUNT(*) FROM "Turns";"""));
            await AssertConflict(connection, Insert(Guid.NewGuid(), medic, occupiedTime));
            await AssertConflict(connection, $"""
                UPDATE "Turns" SET "TimeId" = '{occupiedTime}' WHERE "Id" = '{other}';
                """);
            await Execute(connection, $"""UPDATE "Turns" SET "TimeId" = '{occupiedTime}' WHERE "Id" = '{first}';""");
            var movedTime = Guid.NewGuid();
            await Execute(connection, $"""UPDATE "Turns" SET "TimeId" = '{movedTime}' WHERE "Id" = '{other}';""");
            await AssertConflict(connection, Insert(Guid.NewGuid(), medic, movedTime));
            await Execute(connection, Insert(Guid.NewGuid(), medic, otherTime));
            await Execute(connection, $"""DELETE FROM "Turns" WHERE "Id" = '{first}';""");
            await AssertConflict(connection, Insert(Guid.NewGuid(), medic, occupiedTime));
            await Execute(connection, $"""DELETE FROM "Turns" WHERE "Id" = '{second}';""");
            await Execute(connection, Insert(Guid.NewGuid(), medic, occupiedTime));

            await using (var competing = new NpgsqlConnection(builder.ConnectionString))
            {
                await competing.OpenAsync();
                await using var transaction = await connection.BeginTransactionAsync();
                await Execute(connection, $"""DELETE FROM "Turns" WHERE "Id" = '{duplicateFirst}';""");
                var competingDelete = Execute(competing, $"""DELETE FROM "Turns" WHERE "Id" = '{duplicateSecond}';""");
                await Task.Delay(200);
                Assert.False(competingDelete.IsCompleted);
                await transaction.CommitAsync();
                await competingDelete;
                await Execute(connection, Insert(Guid.NewGuid(), medic, duplicateTime));
            }

            var concurrentTime = Guid.NewGuid();
            await using (var competing = new NpgsqlConnection(builder.ConnectionString))
            {
                await competing.OpenAsync();
                await using var transaction = await connection.BeginTransactionAsync();
                await Execute(connection, Insert(Guid.NewGuid(), medic, concurrentTime));
                var competingInsert = Execute(competing, Insert(Guid.NewGuid(), medic, concurrentTime));
                await Task.Delay(200);
                Assert.False(competingInsert.IsCompleted);
                await transaction.CommitAsync();
                var conflict = await Assert.ThrowsAsync<PostgresException>(() => competingInsert);
                Assert.Equal(PostgresErrorCodes.UniqueViolation, conflict.SqlState);
                Assert.Equal("IX_Turns_MedicId_DateTurn_TimeId", conflict.ConstraintName);
            }

            await Execute(connection, """TRUNCATE "Turns";""");
            await Execute(connection, Insert(Guid.NewGuid(), medic, occupiedTime));
            await migrator.MigrateAsync(previous);
            await Execute(connection, Insert(Guid.NewGuid(), medic, occupiedTime));
        }
        finally
        {
            await Execute(admin, $"""DROP SCHEMA "{schema}" CASCADE;""");
        }
    }

    private static string Insert(Guid id, Guid medic, Guid time) =>
        $"""INSERT INTO "Turns" ("Id", "MedicId", "DateTurn", "TimeId") VALUES ('{id}', '{medic}', '2030-01-07', '{time}');""";

    private static async Task Execute(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<long> Scalar(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    private static async Task AssertConflict(NpgsqlConnection connection, string sql)
    {
        var error = await Assert.ThrowsAsync<PostgresException>(() => Execute(connection, sql));
        Assert.Equal(PostgresErrorCodes.UniqueViolation, error.SqlState);
        Assert.Equal("IX_Turns_MedicId_DateTurn_TimeId", error.ConstraintName);
    }
}
