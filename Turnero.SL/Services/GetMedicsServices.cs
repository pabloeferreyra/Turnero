namespace Turnero.SL.Services;

public class GetMedicsServices(ILoggerServices logger, IMedicRepository medicRepository) : IGetMedicsServices
{
    private readonly ILoggerServices _logger = logger;
    private readonly IMedicRepository _medicRepository = medicRepository;

    public async Task<List<MedicDto>> GetMedicsDto()
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
            //_logger.Debug("Medicos traidos correctamente");
            //});
            var med = await _medicRepository.GetListDto();

            return med;
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return [];
        }
    }

    public async Task<List<Medic>> GetMedics()
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
            //_logger.Debug("Medicos traidos correctamente");
            //});
            var med = await _medicRepository.GetList();

            return med;
        }
        catch (Exception)
        {
            ////_logger.Error(ex.Message, ex);
            return [];
        }
    }

    public async Task<Medic> GetMedicById(Guid id)
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
            //_logger.Debug("Medico traido correctamente");
            //});
            Medic med = await _medicRepository.GetById(id);

            return med;
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return new Medic();
        }
    }

    public async Task<Medic?> GetMedicByUserId(string id)
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
            //_logger.Debug("Medico traido correctamente por usuario");
            //});
            Medic? med = await _medicRepository.GetByUserId(id);

            return med;
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return new Medic();
        }
    }

    public bool ExistMedic(Guid id)
    {
        try
        {
            return _medicRepository.Exists(id);
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
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
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return [];
        }
    }
}
