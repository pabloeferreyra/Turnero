namespace Turnero.Models;

public class TimeTurn 
{ 
    public Guid Id { get; set; } 
    public string Time { get; set; } 
    public ICollection<Turn> Turns { get; set; } 
    public ICollection<Available> Availables { get; set; }
}