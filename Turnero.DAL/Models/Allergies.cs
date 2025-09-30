namespace Turnero.DAL.Models;

public class Allergies : BaseEntity
{
    [Display(Name = "Alergia"), Required]
    public string? Name { get; set; }
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
    public DateTime Begin { get; set; }
    public DateTime? End { get; set; }
    public string? Description { get; set; }
    public Severity Severity { get; set; }
    public AllergyType Type { get; set; }
    public Occurrency Occurrency { get; set; }
    public string? Comments { get; set; }
}
