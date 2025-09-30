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
    public ICollection<Turn>? Turns { get; set; }
}
