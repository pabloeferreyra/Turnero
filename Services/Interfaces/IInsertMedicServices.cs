using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Interfaces
{
    public interface IInsertMedicServices
    {
        Task Create(Medic medic);
    }
}
