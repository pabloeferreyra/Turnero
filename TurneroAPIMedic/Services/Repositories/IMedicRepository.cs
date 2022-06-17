using TurneroAPI.Models;

namespace TurneroAPI.Services.Repositories
{
    public interface IMedicRepository
    {
        Task<List<Medic>> GetList();
        Task<Medic> GetById(Guid id);
        Task<Medic> GetByUserId(string id);
        bool Exists(Guid id);
        Task NewMedic(Medic medic);
        Task DeleteMedic(Medic medic);
        Task UpdateMedic(Medic medic);
    }
}
