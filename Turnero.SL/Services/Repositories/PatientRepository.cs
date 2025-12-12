namespace Turnero.SL.Services.Repositories;

public class PatientRepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<Patient>(context, cache), IPatientRepository
{
    public async Task<List<PatientDTO>> GetList()
    {
        var patients = await FindAll().Include(p => p.ContactInfo).ToListAsync();
        return patients.Adapt<List<PatientDTO>>();
    }

    public IQueryable<PatientDTO> GetAll()
    {
        var patients = FindAll().ToList();
        return patients.Adapt<List<PatientDTO>>().AsQueryable();
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
        DeleteAsync(patient);
    }
    public async Task UpdatePatient(Patient patient)
    {
        await UpdateAsync(patient);
    }
    public async Task<IQueryable<PatientDTO>> SearchByNameOrDni(string search)
    {
        if (search == null)
            return GetAll();
        return FindByCondition(p => (p.Name != null && p.Name.Contains(search)) || p.Dni.Contains(search)).ToList().Adapt<List<PatientDTO>>().AsQueryable();
    }
}

public interface IPatientRepository
{
    Task<List<PatientDTO>> GetList();
    IQueryable<PatientDTO> GetAll();
    Task<Patient> GetById(Guid id);
    bool Exists(string dni, string name);
    Task NewPatient(Patient patient);
    void DeletePatient(Patient patient);
    Task UpdatePatient(Patient patient);
    Task<IQueryable<PatientDTO>> SearchByNameOrDni(string search);
}