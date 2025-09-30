namespace Turnero.DAL.Models;

public class Visit : BaseEntity
{
    public DateTime VisitDate { get; set; }
    public string? Reason { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }
    public Guid MedicId { get; set; }
    public Medic? Medic { get; set; }
}
