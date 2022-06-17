using TurneroAPI.DTO;
using TurneroAPI.Services.Interfaces;
using TurneroAPI.Services.Repositories;

namespace TurneroAPI.Services
{
    public class InsertMedicServices : IInsertMedicServices
    {
        private readonly IMedicRepository _medicRepository;

        public InsertMedicServices(IMedicRepository medicRepository)
        {
            _medicRepository = medicRepository;
        }

        public async Task Create(MedicDTO medic)
        {
            try
            {

                await _medicRepository.NewMedic(medic);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
