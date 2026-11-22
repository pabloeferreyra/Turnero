using Moq;
using Turnero.Application.Common.Interfaces;
using Turnero.Application.Services.TurnsServices;
using Turnero.Domain.Entities;
using Xunit;

namespace Turnero.Test;

public class PublicTurnBookingServiceTests
{
    private readonly Mock<IMedicRepository> medics = new();
    private readonly Mock<ITimeTurnRepository> times = new();
    private readonly Mock<ITurnRepository> turns = new();
    private readonly DateOnly date = new(2030, 1, 7);
    private readonly Guid medicId = Guid.NewGuid();
    private readonly Guid timeId = Guid.NewGuid();

    private PublicTurnBookingService CreateService() =>
        new(medics.Object, times.Object, turns.Object);

    private Turn BuildTurn() => new()
    {
        MedicId = medicId,
        TimeId = timeId,
        DateTurn = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
    };

    private void SetSchedule(string selectedTime, string from, string to)
    {
        times.Setup(repository => repository.GetList())
            .ReturnsAsync([new TimeTurn { Id = timeId, Time = selectedTime }]);
        medics.Setup(repository => repository.GetAvailability(medicId, date))
            .ReturnsAsync([
                new MedicAvailability
                {
                    TimeFrom = new TimeTurn { Time = from },
                    TimeTo = new TimeTurn { Time = to }
                }
            ]);
    }

    [Theory]
    [InlineData("09:00", PublicTurnBookingResult.Created)]
    [InlineData("11:30", PublicTurnBookingResult.Created)]
    [InlineData("11:59", PublicTurnBookingResult.Created)]
    [InlineData("12:00", PublicTurnBookingResult.Unavailable)]
    [InlineData("12:01", PublicTurnBookingResult.Unavailable)]
    [InlineData("08:59", PublicTurnBookingResult.Unavailable)]
    public async Task RespectsAvailabilityBoundaries(string selected, PublicTurnBookingResult expected)
    {
        SetSchedule(selected, "09:00", "12:00");
        var result = await CreateService().CreateAsync(BuildTurn(), date);

        Assert.Equal(expected, result);
        turns.Verify(repository => repository.CreateTurn(It.IsAny<Turn>()),
            expected == PublicTurnBookingResult.Created ? Times.Once : Times.Never);
    }

    [Fact]
    public async Task ReturnsUnavailableWhenDatabaseReportsConcurrentReservation()
    {
        SetSchedule("09:00", "09:00", "12:00");
        turns.Setup(repository => repository.CreateTurn(It.IsAny<Turn>()))
            .ThrowsAsync(new TurnSlotConflictException());

        Assert.Equal(PublicTurnBookingResult.Unavailable,
            await CreateService().CreateAsync(BuildTurn(), date));
    }

    [Fact]
    public async Task RejectsMissingTimeOrOccupiedSlot()
    {
        times.Setup(repository => repository.GetList()).ReturnsAsync([]);
        Assert.Equal(PublicTurnBookingResult.InvalidTime,
            await CreateService().CreateAsync(BuildTurn(), date));

        SetSchedule("09:00", "09:00", "12:00");
        turns.Setup(repository => repository.CheckTurn(medicId, It.IsAny<DateTime>(), timeId))
            .Returns(true);
        Assert.Equal(PublicTurnBookingResult.Unavailable,
            await CreateService().CreateAsync(BuildTurn(), date));
        turns.Verify(repository => repository.CreateTurn(It.IsAny<Turn>()), Times.Never);
    }

    [Fact]
    public async Task DoesNotHideInvalidStoredTime()
    {
        SetSchedule("09:00", "invalid", "12:00");
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateAsync(BuildTurn(), date));
    }

    [Fact]
    public async Task AvailableTimesExcludeBookedSlotsAndRespectExclusiveEnd()
    {
        var earlier = new TimeTurn { Id = Guid.NewGuid(), Time = "09:00" };
        var booked = new TimeTurn { Id = Guid.NewGuid(), Time = "09:30" };
        var later = new TimeTurn { Id = Guid.NewGuid(), Time = "10:00" };
        var end = new TimeTurn { Id = Guid.NewGuid(), Time = "10:30" };
        times.Setup(repository => repository.GetList()).ReturnsAsync([end, later, booked, earlier]);
        medics.Setup(repository => repository.GetAvailability(medicId, date))
            .ReturnsAsync([new MedicAvailability { TimeFrom = earlier, TimeTo = end }]);
        turns.Setup(repository => repository.GetBookedTimeIds(
                medicId, date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)))
            .ReturnsAsync([booked.Id]);

        var result = await CreateService().GetAvailableTimesAsync(medicId, date);

        Assert.Equal(new[] { earlier.Id, later.Id }, result.Select(time => time.Id));
    }

    [Fact]
    public async Task AvailableTimesRejectInvalidStoredRanges()
    {
        SetSchedule("09:00", "12:00", "09:00");
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().GetAvailableTimesAsync(medicId, date));
    }
}
