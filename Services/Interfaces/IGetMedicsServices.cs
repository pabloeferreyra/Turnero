

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Interfaces
{
    public interface IGetMedicsServices
    {
        Task<List<Medic>> GetMedics();
        Task<Medic> GetMedicById(Guid id);
        Task<Medic> GetMedicByUserId(string id);
        bool ExistMedic(Guid id);
    }
}
