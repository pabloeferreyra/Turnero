namespace Turnero.Application.DTOs;

/// <summary>
/// Datos agregados de la historia clínica de un paciente para generación de reportes.
/// La presentación (o un adaptador de reportes) consume este DTO sin conocer EF Core.
/// </summary>
public class PatientClinicalHistoryDto
{
    public Patient? Patient { get; set; }
    public ParentsData? Parent { get; set; }
    public PersonalBackground? PersonalBackground { get; set; }
    public PerinatalBackground? PerinatalBackground { get; set; }
    public CongErrors? CongenitalErrors { get; set; }
    public List<Allergies> Allergies { get; set; } = [];
    public List<Vaccines> Vaccines { get; set; } = [];
    public List<PermMed> PermanentMedication { get; set; } = [];
    public List<GrowthChart> GrowthCharts { get; set; } = [];
    public List<Visit> Visits { get; set; } = [];
    public List<Turn> Turns { get; set; } = [];
}

/// <summary>Resultado agregado del health check del sistema.</summary>
public class SystemHealthStatus
{
    public List<SystemHealthCheck> Checks { get; set; } = [];
    public bool OverallHealthy { get; set; }

    public double? MemoryUsageMb { get; set; }
    public double? MemoryLimitMb { get; set; }
    public double? MemoryPercentage { get; set; }
    public string MemoryStatus { get; set; } = "unknown";
}

/// <summary>Check individual de salud (base de datos, memoria, etc.).</summary>
public class SystemHealthCheck
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "unknown";
    public string? Icon { get; set; }
    public string? Description { get; set; }
}
