using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services
{
    public class GetTurnDTOServices : IGetTurnDTOServices
    {
        private readonly ILoggerServices _logger;
        private readonly ITurnDTORepository _turnRepository;
        private readonly string _connectionString;
        public GetTurnDTOServices(ILoggerServices logger,
                                ITurnDTORepository turnRepository,
                                IConfiguration configuration)
        {
            _logger = logger;
            _turnRepository = turnRepository;
            _connectionString = configuration["ConnectionStrings:PostgresDemoConnection"];
        }

        public IQueryable<TurnDTO> GetTurnsDto()
        {
            try
            {

                return _turnRepository.GetListDto(this._connectionString);
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return null;
            }
        }
    }

    public interface IGetTurnDTOServices
    {
        IQueryable<TurnDTO> GetTurnsDto();
    }
}
