

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TurneroAPI.Models;

namespace TurneroAPI.Services.Interfaces
{
    public interface IGetMedicsServices
    {
        Task<List<Medic>> GetMedics();
        Task<Medic> GetMedicById(Guid id);
        Task<Medic> GetMedicByUserId(string id);
        bool ExistMedic(Guid id);
    }
}
