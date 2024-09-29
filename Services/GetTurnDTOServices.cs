using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services
{
    public class GetTurnDTOServices(ITurnDTORepository turnRepository) : IGetTurnDTOServices
    {
        private readonly string _connectionString = AppSettings.ConnectionString;

        public IQueryable<TurnDTO> GetTurnsDto()
        {
            try
            {

                return turnRepository.GetListDto(this._connectionString);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public interface IGetTurnDTOServices
    {
        IQueryable<TurnDTO> GetTurnsDto();
    }
}
