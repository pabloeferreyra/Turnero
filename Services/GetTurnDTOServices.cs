namespace Turnero.Services
{
    public class GetTurnDTOServices(
                            ITurnDTORepository turnRepository,
                            IConfiguration configuration) : IGetTurnDTOServices
    {
        private readonly ITurnDTORepository _turnRepository = turnRepository;
        private readonly string _connectionString = configuration["ConnectionStrings:PostgresDemoConnection"];

        public IQueryable<TurnDTO> GetTurnsDto()
        {
            try
            {

                return _turnRepository.GetListDto(this._connectionString);
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
