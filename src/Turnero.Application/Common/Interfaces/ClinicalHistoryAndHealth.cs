namespace Turnero.Application.Common.Interfaces;

/// <summary>
/// Puerto de Application: agrega todos los datos de la historia clínica de un paciente.
/// La implementación (EF Core) vive en Infrastructure; la presentación solo consume el DTO.
/// </summary>
public interface IPatientClinicalHistoryService
{
    /// <returns>null si el paciente no existe.</returns>
    Task<PatientClinicalHistoryDto?> GetClinicalHistory(Guid patientId);
}

/// <summary>
/// Puerto de Application: estado de salud del sistema (base de datos, memoria del contenedor).
/// Consumido por la vista admin de Health y por el endpoint /health.
/// </summary>
public interface ISystemHealthService
{
    Task<SystemHealthStatus> GetStatusAsync();
}
