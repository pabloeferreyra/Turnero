namespace Turnero.SL.Services
{
    public class GetTurnsServices(ILoggerServices logger,
                            ITurnRepository turnRepository) : IGetTurnsServices
    {
        private readonly ILoggerServices _logger = logger;
        private readonly ITurnRepository _turnRepository = turnRepository;

        public List<Turn> GetTurns(DateTime? dateTurn, Guid? medicId)
        {
            try
            {
                return _turnRepository.GetList(dateTurn, medicId);
            }
            catch (Exception)
            {
                //_logger.Error(ex.Message, ex);
                return [];
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
                var turn = await _turnRepository.GetById(id);
                return turn ?? new Turn();
            }
            catch (Exception)
            {
                //_logger.Error(ex.Message, ex);
                return new Turn();
            }
        }

        public async Task<TurnDTO> GetTurnDTO(Guid id)
        {
            try
            {
                // _ = Task.Run(() =>
                // {
                //     _logger.Info($"Turno {id}");
                // });
                var dto = await _turnRepository.GetDTOById(id);
                return dto ?? new TurnDTO();
            }
            catch (Exception)
            {
                // _logger.Error(ex.Message, ex);
                return new TurnDTO();
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
