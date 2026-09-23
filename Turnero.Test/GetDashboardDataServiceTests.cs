using Microsoft.Extensions.Caching.Memory;
using Moq;
using Turnero.Application.Common.Interfaces;
using Turnero.Application.Services.TurnsServices;
using Xunit;

namespace Turnero.Test;

public class GetDashboardDataServiceTests
{
    private readonly Mock<ITurnRepository> _turnRepositoryMock;
    private readonly IMemoryCache _memoryCache;
    private readonly GetDashboardDataService _service;

    public GetDashboardDataServiceTests()
    {
        _turnRepositoryMock = new Mock<ITurnRepository>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new GetDashboardDataService(_turnRepositoryMock.Object, _memoryCache);
    }

    private static Turn BuildTurn(DateTime date, string? medicName, string? time, bool accessed) => new()
    {
        Id = Guid.NewGuid(),
        DateTurn = date,
        Accessed = accessed,
        Medic = medicName is null ? null : new Medic { Id = Guid.NewGuid(), Name = medicName },
        Time = time is null ? null : new TimeTurn { Id = Guid.NewGuid(), Time = time }
    };

    private void SetupRepo(List<Turn> turns) =>
        _turnRepositoryMock
            .Setup(r => r.GetTurnsByDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>()))
            .Returns(turns);

    [Fact]
    public void GetDashboardData_NoTurns_ShouldReturnEmptyDto()
    {
        // Arrange
        SetupRepo([]);

        // Act
        var result = _service.GetDashboardData(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        // Assert
        Assert.Equal(0, result.TotalTurns);
        Assert.Equal(0, result.TotalAccessed);
        Assert.Equal(0, result.TotalPending);
        Assert.Empty(result.TurnsPerDay);
        Assert.Empty(result.TurnsPerMedic);
        Assert.Empty(result.TurnsPerMedicPerDay);
        Assert.Empty(result.TurnsPerTimeSlot);
        Assert.Empty(result.DayAccessedBreakdown);
        Assert.Equal(string.Empty, result.BusiestDay);
        Assert.Equal(string.Empty, result.QuietestDay);
        Assert.Equal(string.Empty, result.BusiestMedic);
    }

    [Fact]
    public void GetDashboardData_ShouldAggregateTotalsAccessedAndPending()
    {
        // Arrange: 5 turnos, 3 ingresados, 2 pendientes
        var d1 = new DateTime(2026, 3, 10);
        var turns = new List<Turn>
        {
            BuildTurn(d1, "Dr. García", "09:00", accessed: true),
            BuildTurn(d1, "Dr. García", "09:00", accessed: true),
            BuildTurn(d1, "Dra. López", "11:00", accessed: true),
            BuildTurn(d1, "Dra. López", "11:00", accessed: false),
            BuildTurn(d1, "Dr. García", "09:00", accessed: false)
        };
        SetupRepo(turns);

        // Act
        var result = _service.GetDashboardData(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 10));

        // Assert
        Assert.Equal(5, result.TotalTurns);
        Assert.Equal(3, result.TotalAccessed);
        Assert.Equal(2, result.TotalPending);
        Assert.Single(result.TurnsPerDay);
        Assert.Equal(5, result.TurnsPerDay[0].Count);
        Assert.Single(result.DayAccessedBreakdown);
        Assert.Equal(3, result.DayAccessedBreakdown[0].Accessed);
        Assert.Equal(2, result.DayAccessedBreakdown[0].Pending);
    }

    [Fact]
    public void GetDashboardData_ShouldRankMedicsAndDays()
    {
        // Arrange: García tiene 3 turnos el 10/03; López tiene 1 el 11/03.
        // El día 10/03 es el más ocupado y también el más calmo... no: 11/03 tiene 1.
        var d1 = new DateTime(2026, 3, 10);
        var d2 = new DateTime(2026, 3, 11);
        var turns = new List<Turn>
        {
            BuildTurn(d1, "Dr. García", "09:00", true),
            BuildTurn(d1, "Dr. García", "10:00", false),
            BuildTurn(d1, "Dr. García", "11:00", true),
            BuildTurn(d2, "Dra. López", "09:00", false)
        };
        SetupRepo(turns);

        // Act
        var result = _service.GetDashboardData(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 11));

        // Assert: médico con más turnos
        Assert.Equal("Dr. García", result.BusiestMedic);
        Assert.Equal(3, result.BusiestMedicCount);
        Assert.Equal(2, result.TurnsPerMedic.Count);
        Assert.Equal("Dr. García", result.TurnsPerMedic[0].MedicName); // ordenado desc por count

        // El día con más y menos turnos (sin empates: 3 vs 1)
        Assert.Equal(3, result.BusiestDayCount);
        Assert.Equal(1, result.QuietestDayCount);
    }

    [Fact]
    public void GetDashboardData_ShouldGroupPerMedicPerDay()
    {
        // Arrange
        var d1 = new DateTime(2026, 3, 10);
        var turns = new List<Turn>
        {
            BuildTurn(d1, "Dr. García", "09:00", true),
            BuildTurn(d1, "Dr. García", "09:00", false),
            BuildTurn(d1, "Dra. López", "11:00", true)
        };
        SetupRepo(turns);

        // Act
        var result = _service.GetDashboardData(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 10));

        // Assert
        Assert.Equal(2, result.TurnsPerMedicPerDay.Count);
        var garcia = result.TurnsPerMedicPerDay.Single(m => m.MedicName == "Dr. García");
        Assert.Equal(2, garcia.Count);
        Assert.Equal("2026-03-10", garcia.Date);
    }

    [Fact]
    public void GetDashboardData_ShouldOrderTimeSlots()
    {
        // Arrange: se insertan desordenados; el servicio ordena por string
        var d1 = new DateTime(2026, 3, 10);
        var turns = new List<Turn>
        {
            BuildTurn(d1, "Dr. García", "11:00", true),
            BuildTurn(d1, "Dr. García", "09:00", true),
            BuildTurn(d1, "Dr. García", "09:00", false)
        };
        SetupRepo(turns);

        // Act
        var result = _service.GetDashboardData(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 10));

        // Assert
        Assert.Equal(2, result.TurnsPerTimeSlot.Count);
        Assert.Equal("09:00", result.TurnsPerTimeSlot[0].Time);
        Assert.Equal(2, result.TurnsPerTimeSlot[0].Count);
        Assert.Equal("11:00", result.TurnsPerTimeSlot[1].Time);
    }

    [Fact]
    public void GetDashboardData_NullMedicOrTime_ShouldUseFallbackNames()
    {
        // Arrange
        var d1 = new DateTime(2026, 3, 10);
        var turns = new List<Turn>
        {
            BuildTurn(d1, medicName: null, time: null, accessed: false)
        };
        SetupRepo(turns);

        // Act
        var result = _service.GetDashboardData(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 10));

        // Assert
        Assert.Equal("Sin médico", result.TurnsPerMedic[0].MedicName);
        Assert.Equal("Sin horario", result.TurnsPerTimeSlot[0].Time);
        Assert.Equal("Sin médico", result.BusiestMedic);
    }

    [Fact]
    public void GetDashboardData_ShouldBuildWeeklyAndMonthlySummaries()
    {
        // Arrange: 02/03/2026 (semana ISO 10) y 31/03/2026 (semana ISO 14), mismo mes
        var d1 = new DateTime(2026, 3, 2);
        var d2 = new DateTime(2026, 3, 31);
        var turns = new List<Turn>
        {
            BuildTurn(d1, "Dr. García", "09:00", true),
            BuildTurn(d1, "Dr. García", "10:00", false),
            BuildTurn(d2, "Dra. López", "09:00", true)
        };
        SetupRepo(turns);

        // Act
        var result = _service.GetDashboardData(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        // Assert: 2 semanas distintas, 1 mes
        Assert.Equal(2, result.WeeklySummaries.Count);
        Assert.Single(result.MonthlySummaries);

        var week1 = result.WeeklySummaries[0];
        Assert.Equal(2, week1.Total);
        Assert.Equal(1, week1.Accessed);
        Assert.Equal(1, week1.Pending);
        Assert.Equal(1, week1.DaysInRange);
        Assert.Equal(2.0, week1.AvgPerDay);
        Assert.Equal("02/03", week1.BusiestDay);

        var month = result.MonthlySummaries[0];
        Assert.Equal(3, month.Total);
        Assert.Equal(2, month.Accessed);
        Assert.Equal(1, month.Pending);
        Assert.Equal(2, month.DaysInRange);
        Assert.Equal(1.5, month.AvgPerDay);
        Assert.Contains("2026", month.Label); // el nombre del mes depende de la cultura
    }

    [Fact]
    public void GetDashboardData_SecondCallWithinCache_ShouldNotQueryRepositoryAgain()
    {
        // Arrange
        var d1 = new DateTime(2026, 3, 10);
        SetupRepo([BuildTurn(d1, "Dr. García", "09:00", true)]);
        var start = new DateOnly(2026, 3, 10);
        var end = new DateOnly(2026, 3, 10);

        // Act
        var first = _service.GetDashboardData(start, end);
        var second = _service.GetDashboardData(start, end);

        // Assert: misma instancia cacheada y una sola consulta
        Assert.Same(first, second);
        _turnRepositoryMock.Verify(
            r => r.GetTurnsByDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>()),
            Times.Once);
    }

    [Fact]
    public void GetDashboardData_MedicId_ShouldPassFilterToRepository()
    {
        // Arrange
        SetupRepo([]);
        var medicId = Guid.NewGuid();

        // Act
        _service.GetDashboardData(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 11), medicId);

        // Assert: el filtro llegó intacto al repositorio (TimeOnly.MaxValue = 23:59:59.9999999)
        _turnRepositoryMock.Verify(
            r => r.GetTurnsByDateRange(
                new DateTime(2026, 3, 10, 0, 0, 0),
                new DateOnly(2026, 3, 11).ToDateTime(TimeOnly.MaxValue),
                medicId),
            Times.Once);
    }

    [Fact]
    public void GetDashboardData_NullMedicId_ShouldPassNullToRepository()
    {
        // Arrange
        SetupRepo([]);

        // Act
        _service.GetDashboardData(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 11));

        // Assert
        _turnRepositoryMock.Verify(
            r => r.GetTurnsByDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null),
            Times.Once);
    }
}
