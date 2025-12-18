namespace Turnero.DAL.Models;

public class CongErrors : PatientFKEntity
{
    public bool CongHypothyroidism { get; set; }
    public string? ResultHipot { get; set; }
    public bool Phenylalanine { get; set; }
    public string? ResultFenil { get; set; }
    public bool FQP { get; set; }
    public string? ResultFQP { get; set; }
    public string? Other { get; set; }
    public bool Biotinidase { get; set; }
    public string? ResultBiot { get; set; }
    public bool Galactosemia { get; set; }
    public string? ResultGalac { get; set; }
    public bool OHP { get; set; }
    public string? ResultOHP { get; set; }
    public Patient Patient { get; set; }
}
