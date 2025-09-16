namespace Turnero.SL.Services.Interfaces;

public interface IGetMedicsServices
{
    Task<List<MedicDto>> GetMedicsDto();
    Task<List<Medic>> GetMedics();
    Task<Medic> GetMedicById(Guid id);
    Task<Medic?> GetMedicByUserId(string id);
    bool ExistMedic(Guid id);
    Task<List<MedicDto>> GetCachedMedics();
}
