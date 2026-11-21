namespace Turnero.Domain.Entities;

public class MedicAvailability
{
    public Guid Id { get; set; }
    public Guid MedicId { get; set; }
    public Medic Medic { get; set; } = null!;

    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public DayOfWeek Day { get; set; }

public Guid TimeFromId { get; set; }
public TimeTurn TimeFrom { get; set; } = null!;

public Guid TimeToId { get; set; }
public TimeTurn TimeTo { get; set; } = null!;
}