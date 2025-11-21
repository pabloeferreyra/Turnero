namespace Turnero.DAL.Models;

public class Allergies : BaseEntity
{
    [Display(Name = "Alergia"), Required]
    public string? Name { get; set; }
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
    public DateOnly Begin { get; set; }
    public DateOnly? End { get; set; }
    public string? Description { get; set; }
    public Severity Severity { get; set; }
    public AllergyType Type { get; set; }
    public Occurrency Occurrency { get; set; }
    public string? Comments { get; set; }
}
