namespace Turnero.DAL.Models;

public class Patient : BaseEntity
{
    [Display(Name = "Nombre"), Required]
    public string? Name { get; set; }
    [StringLength(10, MinimumLength = 6), Required]
    public string? Dni { get; set; }
    [Display(Name = "Fecha de Nacimiento"), Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime BirthDate { get; set; }
    public ContactInfo? ContactInfo { get; set; }
    [Display(Name = "Obra Social")]
    public string? SocialWork { get; set; }
    [Display(Name = "Número de Afiliado")]
    public string? AffiliateNumber { get; set; }
    [Display(Name = "Grupo Sanguineo")]
    public BloodType BloodType { get; set; }
    public ICollection<Turn>? Turns { get; set; }
    public ICollection<Visit>? Visits { get; set; }
    public ParentsData? Parent { get; set; }
    public PersonalBackground? PersonalBackground { get; set; }
    public PerinatalBackground? PerinatalBackground { get; set; }
    public ICollection<Vaccines>? Vaccines { get; set; }
    public ICollection<Allergies>? Allergies { get; set; }
}

public class PatientDTO : BaseEntity
{
    [Display(Name = "Nombre")]
    public string? Name { get; set; }
    public int? Dni { get; set; }
    [Display(Name = "Fecha de Nacimiento")]
    public string? BirthDate { get; set; }
    [Display(Name = "Obra Social")]
    public string? SocialWork { get; set; }
    [Display(Name = "Número de Afiliado")]
    public string? AffiliateNumber { get; set; }
}
