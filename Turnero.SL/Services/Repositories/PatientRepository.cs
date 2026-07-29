namespace Turnero.SL.Services.Repositories;

public class PatientRepository(ApplicationDbContext context, IMemoryCache cache, RedisCacheService redisCache) : RepositoryBase<Patient>(context, cache, redisCache), IPatientRepository
{
    public async Task<List<PatientDTO>> GetList()
    {
        return await FindAll().ProjectToType<PatientDTO>().ToListAsync();
    }

    public async Task<List<PatientDTO>> GetCachedPatients()
    {
        return await GetCachedData("patients", GetList);
    }

    public IQueryable<PatientDTO> GetAll()
    {
        return FindAll().ProjectToType<PatientDTO>();
    }
    public async Task<Patient> GetById(Guid id)
    {
        return await FindByCondition(p => p.Id == id)
            .Include(p => p.ContactInfo)
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("No se encontró el paciente con el id especificado.");
    }
    public bool Exists(string dni, string name)
    {
        return FindByCondition(p => p.Dni == dni && p.Name == name).Any();
    }
    public async Task NewPatient(Patient patient)
    {
        if (!Exists(patient.Dni, patient.Name))
        {
            if (!string.IsNullOrEmpty(patient.Name))
            {
                await CreateAsync(patient);
                return;
            }
        }
        throw new InvalidOperationException();
    }
    public void DeletePatient(Patient patient)
    {
        Delete(patient);
    }
    public async Task UpdatePatient(Patient patient)
    {
        await UpdateAsync(patient);
    }
    public Task<IQueryable<PatientDTO>> SearchByNameOrDni(string search)
    {
        if (search == null)
            return Task.FromResult(GetAll());
        return Task.FromResult(FindByCondition(p => (p.Name != null && p.Name.Contains(search)) || p.Dni.Contains(search))
            .ProjectToType<PatientDTO>());
    }
}

public interface IPatientRepository
{
    Task<List<PatientDTO>> GetList();
    Task<List<PatientDTO>> GetCachedPatients();
    IQueryable<PatientDTO> GetAll();
    Task<Patient> GetById(Guid id);
    bool Exists(string dni, string name);
    Task NewPatient(Patient patient);
    void DeletePatient(Patient patient);
    Task UpdatePatient(Patient patient);
    Task<IQueryable<PatientDTO>> SearchByNameOrDni(string search);
}