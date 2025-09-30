namespace Turnero.DAL.Models;

public abstract class FamiliarMedicalCondition : BaseEntity
{
    public bool None { get; set; }
    public bool Other { get; set; }
    public string? OtherDescription { get; set; }

    public Familiar Familiar { get; set; } = null!;
    public Guid FamiliarId { get; set; }
}

public class Cancer : FamiliarMedicalCondition
{
    public bool Breast { get; set; }
    public bool Ovarian { get; set; }
    public bool Uterine { get; set; }
    public bool Prostate { get; set; }
    public bool Colon { get; set; }
    public bool Pancreatic { get; set; }
    public bool Lung { get; set; }
    public bool Melanoma { get; set; }
}

public class Cardiovascular : FamiliarMedicalCondition
{
    public bool Hypertension { get; set; }
    public bool Stroke { get; set; }
    public bool MyocardialInfarction { get; set; }
    public bool Arrhythmia { get; set; }
    public bool Hypercholesterolemia { get; set; }
}

public class Metabolic : FamiliarMedicalCondition
{
    public bool DiabetesMellitus { get; set; }
    public bool ThyroidDisease { get; set; }
    public bool Obesity { get; set; }
}

public class Neurological : FamiliarMedicalCondition
{
    public bool Epilepsy { get; set; }
    public bool Migraine { get; set; }
    public bool Dementia { get; set; }
    public bool Parkinson { get; set; }
}

public class Psychiatric : FamiliarMedicalCondition
{
    public bool Depression { get; set; }
    public bool Anxiety { get; set; }
    public bool BipolarDisorder { get; set; }
    public bool Schizophrenia { get; set; }
}

public class Familiar : BaseEntity
{
    public Cancer? Cancer { get; set; }
    public Cardiovascular? Cardiovascular { get; set; }
    public Metabolic? Metabolic { get; set; }
    public Neurological? Neurological { get; set; }
    public Psychiatric? Psychiatric { get; set; }

    public GeneralHistory GeneralHistory { get; set; } = null!;
    public Guid GeneralHistoryId { get; set; }
}
