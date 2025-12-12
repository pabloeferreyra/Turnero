using System.ComponentModel;

namespace Turnero.DAL.Models;

public class PerinatalBackground : PatientFKEntity
{
    [DisplayName("Gesta")]
    public int Feat { get; set; }
    [DisplayName("Parto")]
    public int Delivery { get; set; }
    [DisplayName("Cesárea")]
    public int Cesarean { get; set; }
    [DisplayName("Aborto")]
    public int Abort { get; set; }
    [DisplayName("Peso")]
    public int Weight { get; set; }
    [DisplayName("Talla")]
    public int Height { get; set; }
    [DisplayName("Perimetro Cefálico")]
    public int CefPer { get; set; }
    [DisplayName("Apgar 1\'")]
    public int Apgar1 { get; set; }
    [DisplayName("Apgar 5\'")]
    public int Apgar5 { get; set; }
    [DisplayName("Edad Gestacional")]
    public int GestAge { get; set; }
    [DisplayName("Patologías")]
    public string? Pathologies { get; set; }
    [DisplayName("Errores Congénitos")]
    public string? CongErrors { get; set; }
    public Patient Patient { get; set; }
}
