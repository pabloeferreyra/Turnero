using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Services.Interfaces
{
    public interface IUpdateTurnsServices
    {
        public void Accessed(Turn turn);
        public void Update(Turn turn);
        public void Delete(Turn turn);
    }
}
