namespace Turnero.DAL.Models;

public class Visit : BaseEntity
{
    public DateTime VisitDate { get; set; }
    public string? Reason { get; set; }
    public string? Diagnosis { get; set; }
    public string? DiagDescription { get; set; }
    public string? Treatment { get; set; }
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }
    public Guid MedicId { get; set; }
    public Medic? Medic { get; set; }
    public string? EvolutionNotes { get; set; }
    public string? LabResults { get; set; }
    public string? OtherStudies { get; set; }
    public string? Observations { get; set; }
}

public class VisitDTO : BaseEntity
{
    public string? VisitDate { get; set; }
    public string? Reason { get; set; }
    public Guid PatientId { get; set; }
    public string? Medic { get; set; }
}
