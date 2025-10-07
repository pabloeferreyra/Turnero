namespace Turnero.SL.Services.Repositories;

public class VisitRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<Visit>(context, cache), IVisitRepository
{
    public async Task<List<Visit>> GetVisitsByMedicAndDate(Guid medicId, DateTime date)
    {
        return await FindByCondition(v => v.MedicId == medicId && v.VisitDate.Date == date.Date)
            .Include(v => v.Patient)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsByPatient(Guid patientId)
    {
        return await FindByCondition(v => v.PatientId == patientId)
            .Include(v => v.Medic)
            .ToListAsync();
    }
    public async Task CreateVisit(Visit visit)
    {
        await CreateAsync(visit);
    }
    public async Task UpdateVisit(Visit visit)
    {
        await UpdateAsync(visit);
    }
    public void DeleteVisit(Visit visit)
    {
        DeleteAsync(visit);
    }
    public async Task<Visit?> GetById(Guid id)
    {
        return await FindByCondition(v => v.Id == id)
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .SingleOrDefaultAsync();
    }
    public bool Exists(Guid id)
    {
        return FindByCondition(v => v.Id == id).Any();
    }
    public async Task<List<Visit>> GetAllVisits()
    {
        return await FindAll()
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsByDate(DateTime date)
    {
        return await FindByCondition(v => v.VisitDate.Date == date.Date)
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsByMedic(Guid medicId)
    {
        return await FindByCondition(v => v.MedicId == medicId)
            .Include(v => v.Patient)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsByDateRange(DateTime startDate, DateTime endDate)
    {
        return await FindByCondition(v => v.VisitDate.Date >= startDate.Date && v.VisitDate.Date <= endDate.Date)
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsByMedicAndDateRange(Guid medicId, DateTime startDate, DateTime endDate)
    {
        return await FindByCondition(v => v.MedicId == medicId && v.VisitDate.Date >= startDate.Date && v.VisitDate.Date <= endDate.Date)
            .Include(v => v.Patient)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsByPatientAndDateRange(Guid patientId, DateTime startDate, DateTime endDate)
    {
        return await FindByCondition(v => v.PatientId == patientId && v.VisitDate.Date >= startDate.Date && v.VisitDate.Date <= endDate.Date)
            .Include(v => v.Medic)
            .ToListAsync();
    }
    public async Task<int> GetVisitCountByMedicAndDate(Guid medicId, DateTime date)
    {
        return await FindByCondition(v => v.MedicId == medicId && v.VisitDate.Date == date.Date).CountAsync();
    }
    public async Task<int> GetVisitCountByPatient(Guid patientId)
    {
        return await FindByCondition(v => v.PatientId == patientId).CountAsync();
    }
    public async Task<int> GetTotalVisitCount()
    {
        return await FindAll().CountAsync();
    }
    public async Task<int> GetVisitCountByDate(DateTime date)
    {
        return await FindByCondition(v => v.VisitDate.Date == date.Date).CountAsync();
    }
    public async Task<int> GetVisitCountByMedic(Guid medicId)
    {
        return await FindByCondition(v => v.MedicId == medicId).CountAsync();
    }
    public async Task<int> GetVisitCountByDateRange(DateTime startDate, DateTime endDate)
    {
        return await FindByCondition(v => v.VisitDate.Date >= startDate.Date && v.VisitDate.Date <= endDate.Date).CountAsync();
    }
    public async Task<int> GetVisitCountByMedicAndDateRange(Guid medicId, DateTime startDate, DateTime endDate)
    {
        return await FindByCondition(v => v.MedicId == medicId && v.VisitDate.Date >= startDate.Date && v.VisitDate.Date <= endDate.Date).CountAsync();
    }
    public async Task<int> GetVisitCountByPatientAndDateRange(Guid patientId, DateTime startDate, DateTime endDate)
    {
        return await FindByCondition(v => v.PatientId == patientId && v.VisitDate.Date >= startDate.Date && v.VisitDate.Date <= endDate.Date).CountAsync();
    }
    public async Task<List<Visit>> GetRecentVisitsByPatient(Guid patientId, int count)
    {
        return await FindByCondition(v => v.PatientId == patientId)
            .Include(v => v.Medic)
            .OrderByDescending(v => v.VisitDate)
            .Take(count)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetRecentVisitsByMedic(Guid medicId, int count)
    {
        return await FindByCondition(v => v.MedicId == medicId)
            .Include(v => v.Patient)
            .OrderByDescending(v => v.VisitDate)
            .Take(count)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsWithDiagnosis()
    {
        return await FindByCondition(v => !string.IsNullOrEmpty(v.Diagnosis))
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsWithTreatment()
    {
        return await FindByCondition(v => !string.IsNullOrEmpty(v.Treatment))
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsByReason(string reason)
    {
        return await FindByCondition(v => v.Reason != null && v.Reason.Contains(reason))
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsByDiagnosis(string diagnosis)
    {
        return await FindByCondition(v => v.Diagnosis != null && v.Diagnosis.Contains(diagnosis))
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsByTreatment(string treatment)
    {
        return await FindByCondition(v => v.Treatment != null && v.Treatment.Contains(treatment))
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsByMedicPatientAndDate(Guid medicId, Guid patientId, DateTime date)
    {
        return await FindByCondition(v => v.MedicId == medicId && v.PatientId == patientId && v.VisitDate.Date == date.Date)
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .ToListAsync();
    }
    public async Task<List<Visit>> GetVisitsByMedicPatientAndDateRange(Guid medicId, Guid patientId, DateTime startDate, DateTime endDate)
    {
        return await FindByCondition(v => v.MedicId == medicId && v.PatientId == patientId && v.VisitDate.Date >= startDate.Date && v.VisitDate.Date <= endDate.Date)
            .Include(v => v.Patient)
            .Include(v => v.Medic)
            .ToListAsync();
    }
}

public interface IVisitRepository
{
    Task<List<Visit>> GetVisitsByMedicAndDate(Guid medicId, DateTime date);
    Task<List<Visit>> GetVisitsByPatient(Guid patientId);
    Task CreateVisit(Visit visit);
    Task UpdateVisit(Visit visit);
    void DeleteVisit(Visit visit);
    Task<Visit?> GetById(Guid id);
    bool Exists(Guid id);
    Task<List<Visit>> GetAllVisits();
    Task<List<Visit>> GetVisitsByDate(DateTime date);
    Task<List<Visit>> GetVisitsByMedic(Guid medicId);
    Task<List<Visit>> GetVisitsByDateRange(DateTime startDate, DateTime endDate);
    Task<List<Visit>> GetVisitsByMedicAndDateRange(Guid medicId, DateTime startDate, DateTime endDate);
    Task<List<Visit>> GetVisitsByPatientAndDateRange(Guid patientId, DateTime startDate, DateTime endDate);
    Task<int> GetVisitCountByMedicAndDate(Guid medicId, DateTime date);
    Task<int> GetVisitCountByPatient(Guid patientId);
    Task<int> GetTotalVisitCount();
    Task<int> GetVisitCountByDate(DateTime date);
    Task<int> GetVisitCountByMedic(Guid medicId);
    Task<int> GetVisitCountByDateRange(DateTime startDate, DateTime endDate);
    Task<int> GetVisitCountByMedicAndDateRange(Guid medicId, DateTime startDate, DateTime endDate);
    Task<int> GetVisitCountByPatientAndDateRange(Guid patientId, DateTime startDate, DateTime endDate);
    Task<List<Visit>> GetRecentVisitsByPatient(Guid patientId, int count);
    Task<List<Visit>> GetRecentVisitsByMedic(Guid medicId, int count);
    Task<List<Visit>> GetVisitsWithDiagnosis();
    Task<List<Visit>> GetVisitsWithTreatment();
    Task<List<Visit>> GetVisitsByReason(string reason);
    Task<List<Visit>> GetVisitsByDiagnosis(string diagnosis);
    Task<List<Visit>> GetVisitsByTreatment(string treatment);
    Task<List<Visit>> GetVisitsByMedicPatientAndDate(Guid medicId, Guid patientId, DateTime date);
    Task<List<Visit>> GetVisitsByMedicPatientAndDateRange(Guid medicId, Guid patientId, DateTime startDate, DateTime endDate);
}