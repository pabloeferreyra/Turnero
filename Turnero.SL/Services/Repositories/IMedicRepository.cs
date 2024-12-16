namespace Turnero.SL.Services.Repositories;

public interface IMedicRepository
{
    Task<List<MedicDto>> GetListDto();
    Task<List<Medic>> GetList();
    Task<Medic> GetById(Guid id);
    Task<Medic> GetByUserId(string id);
    bool Exists(Guid id);
    Task NewMedic(Medic medic);
    void DeleteMedic(Medic medic);
    Task UpdateMedic(Medic medic);
    Task<List<MedicDto>> GetCachedMedics();
}
