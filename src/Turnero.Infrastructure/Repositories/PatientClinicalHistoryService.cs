using Microsoft.EntityFrameworkCore;

namespace Turnero.Infrastructure.Repositories;

/// <summary>
/// Adaptador de Infrastructure: consolida todas las lecturas de la historia
/// clínica usando el DbContext. Application solo ve el DTO agregado.
/// </summary>
public class PatientClinicalHistoryService(ApplicationDbContext context) : IPatientClinicalHistoryService
{
    public async Task<PatientClinicalHistoryDto?> GetClinicalHistory(Guid patientId)
    {
        var patient = await context.Patients
            .AsNoTracking()
            .Include(item => item.ContactInfo)
            .SingleOrDefaultAsync(item => item.Id == patientId);

        if (patient is null)
        {
            return null;
        }

        var dto = new PatientClinicalHistoryDto
        {
            Patient = patient,
            Parent = await context.ParentsData.AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == patientId),
            PersonalBackground = await context.PersonalBackground.AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == patientId),
            PerinatalBackground = await context.PerinatalBackground.AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == patientId),
            CongenitalErrors = await context.CongErrors.AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == patientId),
            Allergies = await context.Allergies.AsNoTracking()
                .Where(item => item.PatientId == patientId).ToListAsync(),
            Vaccines = await context.Vaccines.AsNoTracking()
                .Where(item => item.PatientId == patientId).ToListAsync(),
            PermanentMedication = await context.PermMeds.AsNoTracking()
                .Where(item => item.PatientId == patientId).ToListAsync(),
            GrowthCharts = await context.GrowthCharts.AsNoTracking()
                .Where(item => item.PatientId == patientId).ToListAsync(),
            Visits = await context.Visits
                .AsNoTracking()
                .Include(item => item.Medic)
                .Where(item => item.PatientId == patientId)
                .OrderByDescending(item => item.VisitDate)
                .ToListAsync()
        };

        if (long.TryParse(patient.Dni, out var patientDni))
        {
            dto.Turns = await context.Turns
                .AsNoTracking()
                .Include(item => item.Medic)
                .Include(item => item.Time)
                .Where(item => item.Dni == patientDni)
                .OrderByDescending(item => item.DateTurn)
                .ToListAsync();
        }

        return dto;
    }
}
