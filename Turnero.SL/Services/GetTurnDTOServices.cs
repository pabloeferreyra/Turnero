namespace Turnero.SL.Services
{
    public class GetTurnDTOServices(ITurnDTORepository turnRepository) : IGetTurnDTOServices
    {
        private readonly string _connectionString = AppSettings.ConnectionString;

        public IQueryable<TurnDTO> GetTurnsDto()
        {
            try
            {

                return turnRepository.GetListDto(_connectionString);
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
