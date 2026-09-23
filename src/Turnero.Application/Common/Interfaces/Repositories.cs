namespace Turnero.Application.Common.Interfaces;

/// <summary>
/// Contrato base de acceso a datos. La implementación (EF Core + ADO.NET)
/// vive en Turnero.Infrastructure; Application solo conoce esta abstracción.
/// </summary>
public interface IRepositoryBase<T> where T : class
{
    IQueryable<T> FindAll();
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);

    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    List<T> CallStoredProcedure(string procedureName, params object[] parameters);
    IQueryable<T> CallStoredProcedureDTO(string connectionString, string procedureName);
}

/// <summary>
/// Abstracción de cache-aside para consultas frecuentes (timeTurns, medics, etc.).
/// </summary>
public interface ICachedQueryService
{
    Task<List<TResult>> GetCachedData<TResult>(string cacheKey, Func<Task<List<TResult>>> getDataFunc);
}

public interface ITurnRepository
{
    List<Turn> GetList(DateTime? date, Guid? id);
    Task<Turn> GetById(Guid id);
    Task<TurnDTO> GetDTOById(Guid id);
    bool TurnExists(Guid id);
    bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn);
    void Access(Turn turn);
    void DeleteTurn(Turn turn);
    void UpdateTurn(Turn turn);
    Task CreateTurn(Turn turn);
    /// <summary>
    /// Returns turns within a date range with Medic and Time navigation properties included.
    /// Optionally filters by medic ID. Filtering is performed at the database level via EF Core.
    /// </summary>
    List<Turn> GetTurnsByDateRange(DateTime startDate, DateTime endDate, Guid? medicId = null);
}

public interface ITurnDTORepository
{
    IQueryable<TurnDTO> GetListDto(string connectionString);
    IQueryable<TurnDTO> GetListDtoParam(string connectionString, DateOnly date, Guid? id);
}

public interface ITimeTurnRepository
{
    Task<List<TimeTurn>> GetList();
    IQueryable<TimeTurn> GetQueryable();
    Task<TimeTurn> GetbyId(Guid id);
    bool Exists(Guid id);
    Task CreateTT(TimeTurn timeTurn);

    void DeleteTT(TimeTurn timeTurn);

    Task<List<TimeTurn>> GetCachedTimes();
}

public interface IMedicRepository
{
    Task<List<MedicDto>> GetListDto();
    Task<List<Medic>> GetList();
    Task<Medic> GetById(Guid id);
    Task<Medic?> GetByUserId(string id);
    bool Exists(Guid id);
    Task NewMedic(Medic medic);
    void DeleteMedic(Medic medic);
    Task UpdateMedic(Medic medic);
    Task<List<MedicDto>> GetCachedMedics();
    void InvalidateCachedMedics();
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

public interface IVisitRepository
{
    Task<Visit?> Get(Guid? id);
    Task<IQueryable<VisitDTO>> SearchVisits(Guid patientId);
    Task CreateVisit(Visit visit);
}

public interface IAllergiesRepository
{
    Task<Allergies?> Get(Guid? id);
    Task<List<Allergies>> GetAllergiesByPatient(Guid? id);
    Task CreateAllergy(Allergies allergy);
    Task UpdateAllergy(Allergies allergy);
    void DeleteAllergy(Allergies allergy);
    Task<IQueryable<Allergies>> SearchAllergies(Guid id);
}

public interface IParentsDataRepository
{
    Task<ParentsData?> Get(Guid id);
    Task Update(ParentsData data);
    void Delete(ParentsData data);
}

public interface IPersonalBackgroundRepository
{
    Task<PersonalBackground?> Get(Guid id);
    Task Update(PersonalBackground data);
}

public interface IPerinatalBackgroundRepository
{
    Task<PerinatalBackground?> Get(Guid id);
    Task Update(PerinatalBackground data);
}

public interface IVaccinesRepository
{
    Task<Vaccines?> Get(Guid? id);
    Task<List<Vaccines>> GetByPatientId(Guid patientId);
    Task Update(Vaccines vaccines);
    Task Insert(Vaccines vaccines);
    void Remove(Vaccines vaccines);
}

public interface IPermMedRepository
{
    Task<List<PermMed>> GetByPatientId(Guid patientId);
    Task<PermMed?> GetById(Guid id);
    Task Insert(PermMed permMed);
    Task Remove(Guid id);
}

public interface IGrowthChartRepository
{
    Task<List<GrowthChart>> GetByPatientId(Guid patientId);
    Task<GrowthChart?> GetById(Guid id);
    Task Insert(GrowthChart growthChart);
    Task Edit(GrowthChart growthChart);
    Task Remove(Guid id);
}

public interface ICongErrorsRepository
{
    Task<CongErrors?> Get(Guid id);
    Task Update(CongErrors data);
}
