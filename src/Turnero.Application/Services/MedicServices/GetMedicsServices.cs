namespace Turnero.Application.Services.MedicServices;

public class GetMedicsServices(LoggerService logger, IMedicRepository medicRepository) : IGetMedicsServices
{
    private readonly LoggerService _logger = logger;
    private readonly IMedicRepository _medicRepository = medicRepository;

    public async Task<List<MedicDto>> GetMedicsDto()
    {
        try
        {
            var med = await _medicRepository.GetListDto();

            return med;
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }

    public async Task<List<Medic>> GetMedics()
    {
        try
        {
            var med = await _medicRepository.GetList();

            return med;
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }

    public async Task<Medic> GetMedicById(Guid id)
    {
        try
        {
            Medic med = await _medicRepository.GetById(id);

            return med;
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return new Medic();
        }
    }

    public async Task<Medic?> GetMedicByUserId(string id)
    {
        try
        {
            Medic? med = await _medicRepository.GetByUserId(id);

            return med;
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return new Medic();
        }
    }

    public async Task<List<MedicAvailability>> GetAvailability(Guid id, DateOnly date)
    {
        try
        {
            return await _medicRepository.GetAvailability(id, date);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            throw;
        }
    }

    public async Task<List<MedicAvailability>> GetAvailabilities(Guid id)
    {
        try
        {
            return await _medicRepository.GetAvailabilities(id);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            throw;
        }
    }

    public async Task CreateAvailability(MedicAvailability availability)
    {
        try
        {
            await _medicRepository.CreateAvailability(availability);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            throw;
        }
    }

    public async Task SaveAvailabilities(
        Guid medicId,
        IReadOnlyCollection<MedicAvailability> availabilities,
        IReadOnlyCollection<Guid> unavailableIds)
    {
        try
        {
            await _medicRepository.SaveAvailabilities(medicId, availabilities, unavailableIds);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            throw;
        }
    }

    public bool ExistMedic(Guid id)
    {
        try
        {
            return _medicRepository.Exists(id);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return false;
        }
    }

    public async Task<List<MedicDto>> GetCachedMedics()
    {
        try
        {
            var result = await _medicRepository.GetCachedMedics();
            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            return [];
        }
    }
}

public interface IGetMedicsServices
{
    Task<List<MedicDto>> GetMedicsDto();
    Task<List<Medic>> GetMedics();
    Task<Medic> GetMedicById(Guid id);
    Task<Medic?> GetMedicByUserId(string id);
    bool ExistMedic(Guid id);
    Task<List<MedicDto>> GetCachedMedics();
    Task<List<MedicAvailability>> GetAvailability(Guid id, DateOnly date);
    Task<List<MedicAvailability>> GetAvailabilities(Guid id);
    Task CreateAvailability(MedicAvailability availability);
    Task SaveAvailabilities(
        Guid medicId,
        IReadOnlyCollection<MedicAvailability> availabilities,
        IReadOnlyCollection<Guid> unavailableIds);
}