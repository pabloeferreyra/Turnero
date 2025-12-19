namespace Turnero.DAL.Models;

public class GrowthChart : BaseEntity
{
    [Precision(6, 3)]
    public decimal Age { get; set; }
    public string? Time { get; set; }
    [Precision(13, 3)]
    public decimal Weight { get; set; }
    public string? WPerc { get; set; }
    [Precision(13, 3)]
    public decimal Height { get; set; }
    public string? HPerc { get; set; }
    [Precision(13, 3)]
    public decimal HeadCircumference { get; set; }
    public string? HCPerc { get; set; }
    [Precision(13, 3)]
    public decimal Bmi { get; set; }
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
}