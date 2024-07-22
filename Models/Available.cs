namespace Turnero.Models;

public class Available
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Day {  get; set; }
    [Display(Name = "Médico")]
    public Medic Medic { get; set; }
    public Guid MedicId { get; set; }
    [Display(Name = "Hora")]
    public string Time { get; set; }
    public Guid TimeId { get; set; }
    public Guid TimeStart { get; set; }
    public Guid TimeEnd { get; set; }
}
