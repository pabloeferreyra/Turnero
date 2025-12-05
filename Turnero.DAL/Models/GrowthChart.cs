namespace Turnero.DAL.Models;

public class GrowthChart : BaseEntity
{
    public int Age { get; set; }
    public int Time { get; set; }
    public double Weight { get; set; }
    public int WPerc { get; set; }
    public int Height { get; set; }
    public int HPerc { get; set; }
    public int HeadCircumference { get; set; }
    public int HCPerc { get; set; }
    public double Bmi { get; set; }
    public Patient? Patient { get; set; }
    public Guid PatientId { get; set; }
}

public enum Wperc
{
    PMinus3 = -3,
    P3 = 3,
    P10 = 10,
    P25 = 25,
    P50 = 50,
    P75 = 75,
    P97 = 97,
    PPlus97 = 103
}

public enum Hperc
{
    PMinus3 = -3,
    P3 = 3,
    P10 = 10,
    P25 = 25,
    P50 = 50,
    P75 = 75,
    P97 = 97,
    PPlus97 = 103
}

public enum HCperc
{
    PMinus2Ds = -20,
    P50 = 50,
    PPlus2Ds = 99
}