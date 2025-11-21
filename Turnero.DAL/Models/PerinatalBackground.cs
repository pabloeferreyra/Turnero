namespace Turnero.DAL.Models;

public class PerinatalBackground : PatientFKEntity
{
    public int Feat { get; set; }
    public int Delivery { get; set; }
    public int Cesarean { get; set; }
    public int Abort { get; set; }
    public int Weight { get; set; }
    public int Height { get; set; }
    public int CefPer { get; set; }
    public int Apgar1 { get; set; }
    public int Apgar5 { get; set; }
    public int GestAge { get; set; }
    public string? Pathologies { get; set; }
    public string? CongErrors { get; set; }
}
