namespace Turnero.DAL.Models;

public class PatientFKEntity
{
    [Key]
    [ForeignKey(nameof(Patient))]
    public Guid Id { get; set; }
}
