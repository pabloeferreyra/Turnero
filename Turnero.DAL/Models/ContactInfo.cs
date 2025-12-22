namespace Turnero.DAL.Models;

public class ContactInfo : BaseEntity
{
    [Display(Name = "Teléfono"), Required]
    public string? Phone { get; set; }

    [Display(Name = "Email"), EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "Dirección"), Required]
    public string? Address { get; set; }

    [Display(Name = "Ciudad"), Required]
    public string? City { get; set; }

    [Display(Name = "Código Postal")]
    public string? PostalCode { get; set; }
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
}
