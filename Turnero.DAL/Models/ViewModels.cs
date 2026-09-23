using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Turnero.DAL.Models;

/// <summary>
/// Turns/Index: filter dropdowns. Inherits from TurnDTO so @model
/// DisplayFor expressions keep working; the form is AJAX-driven.
/// </summary>
public class TurnsIndexViewModel
{
    public string? Name { get; set; }
    public List<MedicDto>? Medics { get; set; }
    public string? MedicId { get; set; }
    public bool IsMedic { get; set; }

    public SelectList MedicSelectList => new(Medics ?? [], "Id", "Name");
}

/// <summary>
/// Turns Create (GET modal): TurnDTO fields for binding + dropdown data.
/// </summary>
public class TurnCreateViewModel : TurnDTO
{
    public List<MedicDto>? Medics { get; set; }
    public List<TimeTurn>? Times { get; set; }

    public SelectList MedicSelectList => new(Medics ?? [], "Id", "Name");
    public SelectList TimeSelectList => new(Times ?? [], "Id", "Time");
}

/// <summary>
/// Turns Edit (GET modal): TurnDTO fields for binding + dropdown data.
/// </summary>
public class TurnEditViewModel : TurnDTO
{
    public List<MedicDto>? Medics { get; set; }
    public List<TimeTurn>? Times { get; set; }

    public SelectList MedicSelectList => new(Medics ?? [], "Id", "Name");
    public SelectList TimeSelectList => new(Times ?? [], "Id", "Time");
}

/// <summary>
/// TurnsPublic/Index: public turn request form.
/// </summary>
public class TurnPublicIndexViewModel : TurnDTO
{
    public List<MedicDto>? Medics { get; set; }

    public SelectList MedicSelectList => new(Medics ?? [], "Id", "Name");
}

/// <summary>
/// Dashboard/Index: initial filter values + medics dropdown.
/// </summary>
public class DashboardIndexViewModel
{
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public List<MedicDto>? Medics { get; set; }

    public SelectList MedicSelectList => new(Medics ?? [], "Id", "Name");
}

/// <summary>
/// Patients Create/Edit modals. Inherits from Patient so asp-for /
/// antiforgery / model binding remain unchanged.
/// </summary>
public class PatientFormViewModel : Patient
{
    public PatientFormViewModel() { }

    public PatientFormViewModel(Patient patient)
    {
        Id = patient.Id;
        Name = patient.Name;
        Dni = patient.Dni;
        BirthDate = patient.BirthDate;
        BloodType = patient.BloodType;
        SocialWork = patient.SocialWork;
        AffiliateNumber = patient.AffiliateNumber;
        ContactInfo = patient.ContactInfo;
    }

    public List<SelectListItem> Bloodtypes { get; set; } = [];
}

/// <summary>
/// Patients/Details: patient entity + read-only computed data.
/// </summary>
public class PatientDetailsViewModel
{
    public Patient Patient { get; set; } = new();
    public string Age { get; set; } = string.Empty;
}

public static class BloodTypeSelectList
{
    public static List<SelectListItem> Create() =>
        [.. Enum.GetValues<BloodType>().Select(e => new SelectListItem
        {
            Value = ((int)e).ToString(),
            Text = e.GetType().GetField(e.ToString())
                        ?.GetCustomAttributes(typeof(DisplayAttribute), false)
                        .Cast<DisplayAttribute>()
                        .FirstOrDefault()?.Name ?? e.ToString()
        })];
}

/// <summary>
/// ParentsData details partial: data (null when not created yet) + owning patient id.
/// </summary>
public class ParentsDataDetailsViewModel
{
    public ParentsData? Data { get; set; }
    public Guid PatientId { get; set; }
}

/// <summary>
/// ParentsData Edit modal.
/// </summary>
public class ParentsDataEditViewModel : ParentsData
{
    public List<SelectListItem> FatherBloodtypes { get; set; } = [];
    public List<SelectListItem> MotherBloodtypes { get; set; } = [];

    public ParentsDataEditViewModel() { }

    public ParentsDataEditViewModel(ParentsData data)
    {
        Id = data.Id;
        FatherName = data.FatherName;
        FatherBirthDate = data.FatherBirthDate;
        FatherBloodType = data.FatherBloodType;
        FatherWork = data.FatherWork;
        MotherName = data.MotherName;
        MotherBirthDate = data.MotherBirthDate;
        MotherBloodType = data.MotherBloodType;
        MotherWork = data.MotherWork;
        BrothersCount = data.BrothersCount;
    }
}

/// <summary>
/// Allergies Create/Edit modal. Allergies has no PatientId validation attribute
/// issues; inheriting keeps asp-for paths identical.
/// </summary>
public class AllergyFormViewModel : Allergies
{
    public AllergyFormViewModel() { }

    public AllergyFormViewModel(Allergies allergy)
    {
        Id = allergy.Id;
        PatientId = allergy.PatientId;
        Name = allergy.Name;
        Occurrency = allergy.Occurrency;
        Begin = allergy.Begin;
        End = allergy.End;
        Description = allergy.Description;
        Severity = allergy.Severity;
        Type = allergy.Type;
        Comments = allergy.Comments;
    }

    public List<SelectListItem> Occurrencies { get; set; } = [];
    public List<SelectListItem> Severities { get; set; } = [];
    public List<SelectListItem> Types { get; set; } = [];

    public static List<SelectListItem> OccurrencyList() =>
        [.. Enum.GetValues<Occurrency>().Select(e => new SelectListItem
        {
            Value = ((int)e).ToString(),
            Text = e.ToString()
        })];

    public static List<SelectListItem> SeverityList() =>
        [.. Enum.GetValues<Severity>().Select(e => new SelectListItem
        {
            Value = ((int)e).ToString(),
            Text = e.ToString()
        })];

    public static List<SelectListItem> TypeList() =>
        [.. Enum.GetValues<AllergyType>().Select(e => new SelectListItem
        {
            Value = ((int)e).ToString(),
            Text = e.ToString()
        })];
}

/// <summary>
/// Vaccines Create/Edit modal.
/// </summary>
public class VaccineFormViewModel : Vaccines
{
    public VaccineFormViewModel() { }

    public VaccineFormViewModel(Vaccines vaccine)
    {
        Id = vaccine.Id;
        PatientId = vaccine.PatientId;
        Description = vaccine.Description;
        DateApplied = vaccine.DateApplied;
    }

    public List<SelectListItem> Descriptions { get; set; } = [];
}

/// <summary>
/// CongError Edit modal.
/// </summary>
public class CongErrorEditViewModel : CongErrors
{
    public CongErrorEditViewModel() { }

    public CongErrorEditViewModel(CongErrors data)
    {
        Id = data.Id;
        CongHypothyroidism = data.CongHypothyroidism;
        ResultHypothyroidism = data.ResultHypothyroidism;
        Phenylalanine = data.Phenylalanine;
        ResultPhenylalanine = data.ResultPhenylalanine;
        FQP = data.FQP;
        ResultFQP = data.ResultFQP;
        Biotinidase = data.Biotinidase;
        ResultBiotinidase = data.ResultBiotinidase;
        Galactosemia = data.Galactosemia;
        ResultGalactosemia = data.ResultGalactosemia;
        OHP = data.OHP;
        ResultOHP = data.ResultOHP;
        Other = data.Other;
    }

    public List<SelectListItem> Results { get; set; } = [];

    public static List<SelectListItem> CreateResultList() =>
    [
        new() { Value = CongErrorsResults.NA, Text = CongErrorsResults.NA },
        new() { Value = CongErrorsResults.Normal, Text = CongErrorsResults.Normal },
        new() { Value = CongErrorsResults.Patological, Text = CongErrorsResults.Patological }
    ];
}

/// <summary>
/// Medics/Create: medic fields + selectable medic-role users.
/// </summary>
public class MedicCreateViewModel : Medic
{
    public List<IdentityUser> Users { get; set; } = [];
}

/// <summary>
/// Administration/Health: per-service checks + container memory metrics.
/// </summary>
public class HealthViewModel
{
    public List<HealthCheckResult> Checks { get; set; } = [];
    public string OverallStatus { get; set; } = "unhealthy";
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public double? MemoryUsageMb { get; set; }
    public double? MemoryLimitMb { get; set; }
    public double? MemoryPercentage { get; set; }
    public string MemoryStatus { get; set; } = "unknown";
}

/// <summary>
/// Administration/EditUsersInRole. Kept compatible with the previous
/// List&lt;UserRoleViewModel&gt; POST binding by embedding the list plus RoleId.
/// </summary>
public class EditUsersInRoleViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public List<UserRoleViewModel> Users { get; set; } = [];
}
