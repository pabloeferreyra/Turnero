namespace Turnero.DAL.Models;

public abstract class MedicalExam : BaseEntity
{
    public bool NotApplicable { get; set; }
    public bool Normal { get; set; }
    public bool NotNormal { get; set; }
    public string? Comments { get; set; }
    public ExamsGenHis ExamsGenHis { get; set; } = null!;
    public Guid ExamsGenHisId { get; set; }
}

public class BreastCheck : MedicalExam { }

public class ECGCardiacCheck : MedicalExam { }

public class ECGCheck : MedicalExam { }

public class GynecoCheck : MedicalExam { }

public class MammographyCheck : MedicalExam { }

public class PhysicCheck : MedicalExam { }

public class ProstaticCheck : MedicalExam { }

public class RectalCheck : MedicalExam { }

public class ColonoscopyCheck : MedicalExam { }

public class RetinaCheck : MedicalExam { }

public class FluVaccine : MedicalExam { }

public class PneumoVaccine : MedicalExam { }

public class CholesterolDL : MedicalExam { }

public class Hemoglobin : MedicalExam { }

public class PSACheck : MedicalExam { }

public class ExamsGenHis : BaseEntity
{
    public BreastCheck? BreastCheck { get; set; }
    public ECGCardiacCheck? ECGCardiacCheck { get; set; }
    public ECGCheck? ECGCheck { get; set; }
    public GynecoCheck? GynecoCheck { get; set; }
    public MammographyCheck? MammographyCheck { get; set; }
    public PhysicCheck? PhysicCheck { get; set; }
    public ProstaticCheck? ProstaticCheck { get; set; }
    public RectalCheck? RectalCheck { get; set; }
    public ColonoscopyCheck? ColonoscopyCheck { get; set; }
    public RetinaCheck? RetinaCheck { get; set; }
    public FluVaccine? FluVaccine { get; set; }
    public PneumoVaccine? PneumoVaccine { get; set; }
    public CholesterolDL? CholesterolDL { get; set; }
    public Hemoglobin? Hemoglobin { get; set; }
    public PSACheck? PSACheck { get; set; }
    public GeneralHistory GeneralHistory { get; set; } = null!;
    public Guid GeneralHistoryId { get; set; }
}