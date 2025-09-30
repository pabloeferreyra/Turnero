namespace Turnero.DAL.Models;

// Clase base para miembros de la familia
public abstract class FamilyMember : BaseEntity
{
    public bool Alive { get; set; }
    public string Diagnosis { get; set; } = string.Empty;

    public FamilyBackground FamilyBackground { get; set; } = null!;
    public Guid FamilyBackgroundId { get; set; }
}

public class Father : FamilyMember
{
}

public class Mother : FamilyMember
{
}

public class Siblings : FamilyMember
{
    public int NumberOfSiblings { get; set; }
}

public class Children : FamilyMember
{
    public int NumberOfChildren { get; set; }
}

public class Others : FamilyMember
{
    public string Relation { get; set; } = string.Empty;
}

public class FamilyBackground : BaseEntity
{
    public Father? Father { get; set; }
    public Mother? Mother { get; set; }
    public Siblings? Siblings { get; set; }
    public Children? Children { get; set; }
    public List<Others>? Others { get; set; }
    public GeneralHistory GeneralHistory { get; set; } = null!;
    public Guid GeneralHistoryId { get; set; }
}