namespace Turnero.DAL.Models;

public class History : BaseEntity
{
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
    public FamilyBackground? FamilyBackground { get; set; }
    public GeneralHistory? GeneralHistory { get; set; }
    public Familiar? Familiar { get; set; }
    public Lifestyle? Lifestyle { get; set; }
}
