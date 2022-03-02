using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Interfaces
{
    public interface IInsertTimeTurnServices
    {
        Task Create(TimeTurnViewModel timeTurnViewModel);
    }
}
