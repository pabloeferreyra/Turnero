

namespace Turnero.SL.Services
{
    public class GetTurnsServices : IGetTurnsServices
    {
        private readonly ILoggerServices _logger;
        private readonly ITurnRepository _turnRepository;
        public GetTurnsServices(ILoggerServices logger,
                                ITurnRepository turnRepository)
        {
            _logger = logger;
            _turnRepository = turnRepository;
        }
        public List<Turn> GetTurns(DateTime? dateTurn, Guid? medicId)
        {
            try
            {
                _ = Task.Run(() =>
                {
                    //_logger.Info($"{dateTurn} Turnos llegaron correctamente");
                    return Task.CompletedTask;
                });
                return _turnRepository.GetList(dateTurn, medicId);
            }
            catch (Exception)
            {
                //_logger.Error(ex.Message, ex);
                return null;
            }
        }

        public async Task<Turn> GetTurn(Guid id)
        {
            try
            {
                _ = Task.Run(() =>
                {
                    //_logger.Info($"Turno {id}");
                });
                return await _turnRepository.GetById(id);
            }
            catch (Exception)
            {
                //_logger.Error(ex.Message, ex);
                return null;
            }
        }

        public async Task<TurnDTO> GetTurnDTO(Guid id)
        {
            try
            {
                //_ = Task.Run(() =>
                //{
                //    _logger.Info($"Turno {id}");
                //});
                return await _turnRepository.GetDTOById(id);
            }
            catch (Exception)
            {
                //_logger.Error(ex.Message, ex);
                return null;
            }
        }

        public bool Exists(Guid id)
        {
            try
            {
                return _turnRepository.TurnExists(id);
            }
            catch (Exception)
            {
                //_logger.Error(ex.Message, ex);
                return false;
            }
        }

        public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
        {
            try
            {
                return _turnRepository.CheckTurn(medicId, date, timeTurn);
            }
            catch (Exception)
            {
                //_logger.Error(ex.Message, ex);
                return false;
            }
        }
    }
}
