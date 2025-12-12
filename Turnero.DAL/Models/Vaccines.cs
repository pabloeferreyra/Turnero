namespace Turnero.DAL.Models;

public class Vaccines : BaseEntity
{
    public string? Description { get; set; }
    public DateOnly? DateApplied { get; set; }
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
}


public class VaccinesDto : BaseEntity
{
    public string? Description { get; set; }
    public string? OtherDescription { get; set; }
    public DateOnly? DateApplied { get; set; }
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
}