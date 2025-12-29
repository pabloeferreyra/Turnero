namespace Turnero.DAL.Models;

public class ContactInfo : BaseEntity
{
    [Display(Name = "Teléfono")]
    public string? Phone { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Dirección")]
    public string? Address { get; set; }

    [Display(Name = "Ciudad")]
    public string? City { get; set; }

    [Display(Name = "Código Postal")]
    public string? PostalCode { get; set; }
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
}
