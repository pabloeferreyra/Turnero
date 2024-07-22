namespace Turnero.Services;

public class GetMedicsServices(IMedicRepository medicRepository) : IGetMedicsServices
{
    private readonly IMedicRepository _medicRepository = medicRepository;

    public async Task<List<MedicDto>> GetMedicsDto()
    {
        try
        {
            var med = await _medicRepository.GetListDto();

            return med;
        }
        catch (Exception)
        {
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
        catch (Exception)
        {
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
        catch (Exception)
        {
            return new Medic();
        }
    }

    public async Task<Medic> GetMedicByUserId(string id)
    {
        try
        {
            Medic med = await _medicRepository.GetByUserId(id);

            return med;
        }
        catch (Exception)
        {
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
            return false;
        }
    }

    public async Task<List<MedicDto>> GetCachedMedics()
    {
        try
        {
            return await _medicRepository.GetCachedMedics();
        }
        catch (Exception)
        {
            return null;
        }
    }
}
