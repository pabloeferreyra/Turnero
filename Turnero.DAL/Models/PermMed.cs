namespace Turnero.DAL.Models;

public class PermMed : BaseEntity
{
    public string? Description { get; set; }
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
}
