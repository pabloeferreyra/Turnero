using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using TurneroAPI.Data;
using TurneroAPI.Models;
using TurneroAPI.Services.Interfaces;
using TurneroAPI.Services.Repositories;

namespace TurneroAPI.Services
{
    public class UpdateMedicServices : IUpdateMedicServices
    {
        private readonly IMedicRepository _medicRepository;

        public UpdateMedicServices(IMedicRepository medicRepository)
        {
            _medicRepository = medicRepository;
        }

        public async Task<bool> Update(Medic medic)
        {
            try
            {
                await _medicRepository.UpdateMedic(medic);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return false;
            }
        }

        public async Task Delete(Medic medic)
        {
            try
            {
                await _medicRepository.DeleteMedic(medic);
            }
            catch (DbUpdateConcurrencyException ex)
            {
            }
        }
    }
}
