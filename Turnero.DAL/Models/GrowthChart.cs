namespace Turnero.DAL.Models;

public class GrowthChart : BaseEntity
{
    public double Age { get; set; }
    public string? Time { get; set; }
    public double Weight { get; set; }
    public string? WPerc { get; set; }
    public double Height { get; set; }
    public string? HPerc { get; set; }
    public double HeadCircumference { get; set; }
    public string? HCPerc { get; set; }
    public double Bmi { get; set; }
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
}

public enum HCperc
{
    PMinus2Ds = -20,
    P50 = 50,
    PPlus2Ds = 99
}