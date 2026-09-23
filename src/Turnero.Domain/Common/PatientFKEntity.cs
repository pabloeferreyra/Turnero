namespace Turnero.Domain.Entities;

public class PatientFKEntity
{
    [Key]
    [ForeignKey(nameof(Patient))]
    public Guid Id { get; set; }
}
