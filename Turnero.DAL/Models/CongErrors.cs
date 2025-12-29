using System.ComponentModel;

namespace Turnero.DAL.Models;

public class CongErrors : PatientFKEntity
{
    [DisplayName("Hipotiroidismo congénito")]
    public bool CongHypothyroidism { get; set; }
    public string? ResultHypothyroidism { get; set; }
    [DisplayName("Fenilcetonuria")]
    public bool Phenylalanine { get; set; }
    public string? ResultPhenylalanine { get; set; }
    [DisplayName("Fibrosis quística del páncreas")]
    public bool FQP { get; set; }
    public string? ResultFQP { get; set; }
    [DisplayName("Otras")]
    public string? Other { get; set; }
    [DisplayName("Deficiencia de biotinidasa")]
    public bool Biotinidase { get; set; }
    public string? ResultBiotinidase { get; set; }
    [DisplayName("Galactosemia")]
    public bool Galactosemia { get; set; }
    public string? ResultGalactosemia { get; set; }
    [DisplayName("17O Hidroxiprogesterona Neo")]
    public bool OHP { get; set; }
    public string? ResultOHP { get; set; }
    public Patient Patient { get; set; }
}

public class CongErrorsResults
{
    public const string NA = "-";
    public const string Normal = "Normal";
    public const string Patological = "Patológico";
}
