using System.Linq;
using System;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;
using Microsoft.Extensions.Configuration;

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
            _connectionString = configuration["ConnectionStrings:DefaultConnection"];
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
