namespace Turnero.Application.Services.TurnsServices;

public enum PublicTurnBookingResult
{
    Created,
    InvalidTime,
    Unavailable
}

public interface IPublicTurnBookingService
{
    Task<List<MedicDto>> GetMedicsAsync();
    Task<List<TimeTurn>> GetAvailableTimesAsync(Guid medicId, DateOnly date);
    Task<PublicTurnBookingResult> CreateAsync(Turn turn, DateOnly date);
}

public sealed class PublicTurnBookingService(
    IMedicRepository medics,
    ITimeTurnRepository times,
    ITurnRepository turns) : IPublicTurnBookingService
{
    public Task<List<MedicDto>> GetMedicsAsync() => medics.GetCachedMedics();

    public async Task<List<TimeTurn>> GetAvailableTimesAsync(Guid medicId, DateOnly date)
    {
        var ranges = GetRanges(await medics.GetAvailability(medicId, date));
        if (ranges.Count == 0)
        {
            return [];
        }

        var booked = (await turns.GetBookedTimeIds(
            medicId, date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))).ToHashSet();
        return (await times.GetList())
            .Select(time => (Time: time, Value: ParseTime(time)))
            .Where(item => !booked.Contains(item.Time.Id)
                && ranges.Any(range => item.Value >= range.Start && item.Value <= range.End))
            .OrderBy(item => item.Value)
            .Select(item => item.Time)
            .ToList();
    }

    public async Task<PublicTurnBookingResult> CreateAsync(Turn turn, DateOnly date)
    {
        var time = (await times.GetList()).SingleOrDefault(item => item.Id == turn.TimeId);
        if (time is null)
        {
            return PublicTurnBookingResult.InvalidTime;
        }

        var selectedTime = ParseTime(time);
        var ranges = GetRanges(await medics.GetAvailability(turn.MedicId, date));
        var withinAvailability = ranges.Any(range => selectedTime >= range.Start && selectedTime <= range.End);

        if (!withinAvailability || turns.CheckTurn(turn.MedicId, turn.DateTurn, turn.TimeId))
        {
            return PublicTurnBookingResult.Unavailable;
        }

        try
        {
            await turns.CreateTurn(turn);
            return PublicTurnBookingResult.Created;
        }
        catch (TurnSlotConflictException)
        {
            return PublicTurnBookingResult.Unavailable;
        }
    }

    private static List<(TimeOnly Start, TimeOnly End)> GetRanges(List<MedicAvailability> availabilities)
    {
        var ranges = new List<(TimeOnly Start, TimeOnly End)>();
        foreach (var availability in availabilities)
        {
            var start = ParseTime(availability.TimeFrom);
            var end = ParseTime(availability.TimeTo);
            if (start >= end)
            {
                throw new InvalidOperationException("El rango de disponibilidad del médico no es válido.");
            }

            ranges.Add((start, end));
        }

        return ranges;
    }

    private static TimeOnly ParseTime(TimeTurn time)
    {
        if (!TimeOnly.TryParse(time.Time, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
        {
            throw new InvalidOperationException($"El horario {time.Id} no tiene un formato válido.");
        }

        return value;
    }
}
