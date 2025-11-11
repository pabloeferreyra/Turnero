namespace Turnero.DAL.Models;

public class ParentsData : BaseEntity
{
    public string? FatherName { get; set; }
    public DateOnly FatherBirthDate { get; set; }
    public BloodType FatherBloodType { get; set; }
    public string? FatherWork { get; set; }

    public string? MotherName { get; set; }
    public DateOnly MotherBirthDate { get; set; }
    public BloodType MotherBloodType { get; set; }
    public string? MotherWork { get; set; }
    public int BrothersCount { get; set; }
    public Guid? PatientId { get; set; }
    public Patient? Patient { get; set; }
}
