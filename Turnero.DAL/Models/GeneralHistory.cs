namespace Turnero.DAL.Models;

public class GeneralHistory : BaseEntity
{
    public History? History { get; set; }
    public Guid HistoryId { get; set; }
    public string? RiskFactors { get; set; }
    public ExamsGenHis? ExamsGenHis { get; set; }
    

}
