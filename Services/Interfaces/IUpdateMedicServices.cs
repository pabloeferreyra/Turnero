using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Interfaces
{
    public interface IUpdateMedicServices
    {
        Task<bool> Update(Medic medic);
        Task Delete(Medic medic);
    }
}
