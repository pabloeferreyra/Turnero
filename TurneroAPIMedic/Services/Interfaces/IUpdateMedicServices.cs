using TurneroAPI.Models;

namespace TurneroAPI.Services.Interfaces
{
    public interface IUpdateMedicServices
    {
        Task<bool> Update(Medic medic);
        Task Delete(Medic medic);
    }
}
