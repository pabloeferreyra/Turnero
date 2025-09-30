namespace Turnero.DAL.Models;

public class Lifestyle : BaseEntity
{
    public GeneralHistory? GeneralHistory { get; set; }
    public Guid GeneralHistoryId { get; set; }
    public bool Smoking { get; set; }
    public int CigarettesPerDay { get; set; }
    public int YearsSmoking { get; set; }
    public bool Alcohol { get; set; }
    public string AlcoholType { get; set; } = string.Empty;
    public int DrinksPerWeek { get; set; }
    public bool Drugs { get; set; }
    public string DrugType { get; set; } = string.Empty;
    public int TimesPerWeek { get; set; }
    public bool DangerousActivities { get; set; }
    public string DangerousActivitiesDescription { get; set; } = string.Empty;
    public bool Excercise { get; set; }
    public string ExcerciseType { get; set; } = string.Empty;
    public int TimesPerWeekExcercise { get; set; }
}
