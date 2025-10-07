namespace Turnero.SL.Services.Repositories;

public class PatientRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<Patient>(context, cache), IPatientRepository
{
    public async Task<List<PatientDTO>> GetList()
    {
        var patients = await FindAll().ToListAsync();
        return patients.Adapt<List<PatientDTO>>();
    }

    public IQueryable<PatientDTO> GetAll()
    {
        var patients = FindAll().ToList();
        return patients.Adapt<List<PatientDTO>>().AsQueryable();
    }
    public async Task<Patient> GetById(Guid id)
    {
        return await FindByCondition(p => p.Id == id).SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("No se encontró el paciente con el id especificado.");
    }
    public bool Exists(Guid id)
    {
        return FindByCondition(p => p.Id == id).Any();
    }
    public async Task NewPatient(Patient patient)
    {
        if (!string.IsNullOrEmpty(patient.Name))
        {
            await CreateAsync(patient);
        }
    }
    public void DeletePatient(Patient patient)
    {
        DeleteAsync(patient);
    }
    public async Task UpdatePatient(Patient patient)
    {
        await UpdateAsync(patient);
    }
    public async Task<List<PatientDTO>> SearchByNameOrDni(string search)
    {
        var patients = await FindByCondition(p => p.Name != null && p.Name.Contains(search) || p.Dni.Contains(search)).ToListAsync();
        return patients.Adapt<List<PatientDTO>>();
    }

}

public interface IPatientRepository
{
    Task<List<PatientDTO>> GetList();
    IQueryable<PatientDTO> GetAll();
    Task<Patient> GetById(Guid id);
    bool Exists(Guid id);
    Task NewPatient(Patient patient);
    void DeletePatient(Patient patient);
    Task UpdatePatient(Patient patient);
    Task<List<PatientDTO>> SearchByNameOrDni(string search);
}