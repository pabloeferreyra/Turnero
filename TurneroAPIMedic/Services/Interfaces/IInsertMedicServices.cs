using TurneroAPI.DTO;

namespace TurneroAPI.Services.Interfaces
{
    public interface IInsertMedicServices
    {
        Task Create(MedicDTO medic);
    }
}
