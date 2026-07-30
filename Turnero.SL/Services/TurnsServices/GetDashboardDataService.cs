namespace Turnero.SL.Services.TurnsServices;

public class DashboardDataDto
{
    public List<DateCount> TurnsPerDay { get; set; } = [];
    public List<MedicCount> TurnsPerMedic { get; set; } = [];
    public List<MedicDayCount> TurnsPerMedicPerDay { get; set; } = [];
    public List<TimeSlotCount> TurnsPerTimeSlot { get; set; } = [];
    public List<DayAccessedCount> DayAccessedBreakdown { get; set; } = [];
    public List<WeekSummary> WeeklySummaries { get; set; } = [];
    public List<MonthSummary> MonthlySummaries { get; set; } = [];
    public int TotalTurns { get; set; }
    public int TotalAccessed { get; set; }
    public int TotalPending { get; set; }
    public string BusiestDay { get; set; } = "";
    public int BusiestDayCount { get; set; }
    public string QuietestDay { get; set; } = "";
    public int QuietestDayCount { get; set; }
    public string BusiestMedic { get; set; } = "";
    public int BusiestMedicCount { get; set; }
}

public class TimeSlotCount
{
    public string Time { get; set; } = "";
    public int Count { get; set; }
}

public class DayAccessedCount
{
    public string Date { get; set; } = "";
    public string DisplayDate { get; set; } = "";
    public int Total { get; set; }
    public int Accessed { get; set; }
    public int Pending { get; set; }
}

public class DateCount
{
    public string Date { get; set; } = "";
    public string DisplayDate { get; set; } = "";
    public int Count { get; set; }
}

public class MedicCount
{
    public string MedicName { get; set; } = "";
    public int Count { get; set; }
}

public class MedicDayCount
{
    public string Date { get; set; } = "";
    public string DisplayDate { get; set; } = "";
    public string MedicName { get; set; } = "";
    public int Count { get; set; }
}

public class WeekSummary
{
    public string Label { get; set; } = "";
    public int Total { get; set; }
    public double AvgPerDay { get; set; }
    public int Accessed { get; set; }
    public int Pending { get; set; }
    public int DaysInRange { get; set; }
    public string BusiestDay { get; set; } = "";
    public int BusiestDayCount { get; set; }
}

public class MonthSummary
{
    public string Label { get; set; } = "";
    public int Total { get; set; }
    public double AvgPerDay { get; set; }
    public int Accessed { get; set; }
    public int Pending { get; set; }
    public int DaysInRange { get; set; }
    public string BusiestDay { get; set; } = "";
    public int BusiestDayCount { get; set; }
}

public class DashboardFilterDto
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

public interface IGetDashboardDataService
{
    DashboardDataDto GetDashboardData(DateOnly startDate, DateOnly endDate, Guid? medicId = null);
}

/// <summary>
/// Servicio optimizado que filtra turnos a nivel de base de datos usando EF Core
/// en vez de cargar todos los registros en memoria.
/// </summary>
public class GetDashboardDataService(ITurnRepository turnRepository, IMemoryCache memoryCache) : IGetDashboardDataService
{
    public DashboardDataDto GetDashboardData(DateOnly startDate, DateOnly endDate, Guid? medicId = null)
    {
        var cacheKey = $"dashboard:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}:{medicId?.ToString() ?? "all"}";

        // Check memory cache first
        if (memoryCache.TryGetValue(cacheKey, out DashboardDataDto? cached) && cached != null)
            return cached;

        // ── Consulta eficiente: filtra en la DB con DateTurn (columna DateTime) ──
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

        var turns = turnRepository.GetTurnsByDateRange(startDateTime, endDateTime, medicId);

        var totalTurns = turns.Count;
        var totalAccessed = turns.Count(t => t.Accessed);
        var totalPending = totalTurns - totalAccessed;

        // Turns per day
        var turnsPerDay = turns
            .GroupBy(t => DateOnly.FromDateTime(t.DateTurn))
            .Select(g => new DateCount
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                DisplayDate = g.Key.ToString("dd/MM (ddd)"),
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        var medicName = (Turn t) => t.Medic?.Name ?? "Sin médico";
        var timeSlot = (Turn t) => t.Time?.Time ?? "Sin horario";

        // Turns per medic
        var turnsPerMedic = turns
            .GroupBy(t => medicName(t))
            .Select(g => new MedicCount
            {
                MedicName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(m => m.Count)
            .ToList();

        // Turns per medic per day (for grouped chart)
        var turnsPerMedicPerDay = turns
            .GroupBy(t => new { Date = DateOnly.FromDateTime(t.DateTurn), MedicName = medicName(t) })
            .Select(g => new MedicDayCount
            {
                Date = g.Key.Date.ToString("yyyy-MM-dd"),
                DisplayDate = g.Key.Date.ToString("dd/MM"),
                MedicName = g.Key.MedicName,
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ThenBy(d => d.MedicName)
            .ToList();

        // Turns per time slot
        var turnsPerTimeSlot = turns
            .GroupBy(t => timeSlot(t))
            .Select(g => new TimeSlotCount
            {
                Time = g.Key,
                Count = g.Count()
            })
            .OrderBy(t => t.Time)
            .ToList();

        // Day-accessed breakdown
        var dayAccessedBreakdown = turns
            .GroupBy(t => DateOnly.FromDateTime(t.DateTurn))
            .Select(g => new DayAccessedCount
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                DisplayDate = g.Key.ToString("dd/MM (ddd)"),
                Total = g.Count(),
                Accessed = g.Count(t => t.Accessed),
                Pending = g.Count(t => !t.Accessed)
            })
            .OrderBy(d => d.Date)
            .ToList();

        // Busiest / quietest day
        var busiestDay = turnsPerDay.OrderByDescending(d => d.Count).FirstOrDefault();
        var quietestDay = turnsPerDay.OrderBy(d => d.Count).FirstOrDefault();

        // Busiest medic
        var busiestMedic = turnsPerMedic.FirstOrDefault();

        // ── Weekly summaries ──
        var weeklySummaries = turns
            .GroupBy(t => ISOWeek.GetYear(t.DateTurn) * 100 + ISOWeek.GetWeekOfYear(t.DateTurn))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var weekYear = g.Key / 100;
                var weekNum = g.Key % 100;
                var daysInWeek = g
                    .GroupBy(t => DateOnly.FromDateTime(t.DateTurn))
                    .ToList();
                var busiest = daysInWeek.OrderByDescending(d => d.Count()).First();
                return new WeekSummary
                {
                    Label = $"Sem {weekNum} ({weekYear})",
                    Total = g.Count(),
                    Accessed = g.Count(t => t.Accessed),
                    Pending = g.Count(t => !t.Accessed),
                    AvgPerDay = Math.Round((double)g.Count() / daysInWeek.Count, 1),
                    DaysInRange = daysInWeek.Count,
                    BusiestDay = busiest.Key.ToString("dd/MM"),
                    BusiestDayCount = busiest.Count()
                };
            })
            .ToList();

        // ── Monthly summaries ──
        var monthlySummaries = turns
            .GroupBy(t => new { t.DateTurn.Year, t.DateTurn.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g =>
            {
                var daysInMonth = g
                    .GroupBy(t => DateOnly.FromDateTime(t.DateTurn))
                    .ToList();
                var busiest = daysInMonth.OrderByDescending(d => d.Count()).First();
                var monthName = DateTimeFormatInfo.CurrentInfo.GetMonthName(g.Key.Month);
                return new MonthSummary
                {
                    Label = $"{monthName} {g.Key.Year}",
                    Total = g.Count(),
                    Accessed = g.Count(t => t.Accessed),
                    Pending = g.Count(t => !t.Accessed),
                    AvgPerDay = Math.Round((double)g.Count() / daysInMonth.Count, 1),
                    DaysInRange = daysInMonth.Count,
                    BusiestDay = busiest.Key.ToString("dd/MM"),
                    BusiestDayCount = busiest.Count()
                };
            })
            .ToList();

        var result = new DashboardDataDto
        {
            TurnsPerDay = turnsPerDay,
            TurnsPerMedic = turnsPerMedic,
            TurnsPerMedicPerDay = turnsPerMedicPerDay,
            TurnsPerTimeSlot = turnsPerTimeSlot,
            DayAccessedBreakdown = dayAccessedBreakdown,
            WeeklySummaries = weeklySummaries,
            MonthlySummaries = monthlySummaries,
            TotalTurns = totalTurns,
            TotalAccessed = totalAccessed,
            TotalPending = totalPending,
            BusiestDay = busiestDay?.DisplayDate ?? "",
            BusiestDayCount = busiestDay?.Count ?? 0,
            QuietestDay = quietestDay?.DisplayDate ?? "",
            QuietestDayCount = quietestDay?.Count ?? 0,
            BusiestMedic = busiestMedic?.MedicName ?? "",
            BusiestMedicCount = busiestMedic?.Count ?? 0
        };

        // Cache result for 2 minutes (dashboard data changes infrequently)
        memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(2));

        return result;
    }
}
