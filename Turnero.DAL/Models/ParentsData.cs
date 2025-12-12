namespace Turnero.DAL.Models;

public class ParentsData : PatientFKEntity
{
    [Display(Name = "Nombre del padre")]
    public string? FatherName { get; set; }
    [Display(Name = "Fecha de nacimiento del padre")]
    public DateOnly FatherBirthDate { get; set; }
    [Display(Name = "Tipo sanguineo del padre")]
    public BloodType FatherBloodType { get; set; }
    [Display(Name = "Trabajo del padre")]
    public string? FatherWork { get; set; }
    [Display(Name = "Nombre de la madre")]
    public string? MotherName { get; set; }
    [Display(Name = "Fecha de nacimiento de la madre")]
    public DateOnly MotherBirthDate { get; set; }
    [Display(Name = "Tipo sanguineo de la madre")]
    public BloodType MotherBloodType { get; set; }
    [Display(Name = "Trabajo de la madre")]
    public string? MotherWork { get; set; }
    [Display(Name = "Cantidad de hermanos")]
    public int BrothersCount { get; set; }
    public Patient Patient { get; set; }
}
