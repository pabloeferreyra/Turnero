namespace Turnero.SL.Services
{
    public class GetTurnDTOServices(ITurnDTORepository turnRepository) : IGetTurnDTOServices
    {
        private readonly string _connectionString = AppSettings.ConnectionString ?? throw new InvalidOperationException("ConnectionString no puede ser nulo.");

        public IQueryable<TurnDTO> GetTurnsDto()
        {
            try
            {
                return turnRepository.GetListDto(_connectionString);
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Error al obtener TurnDTOs.", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error inesperado al obtener TurnDTOs.", ex);
            }
        }
    }

    public interface IGetTurnDTOServices
    {
        IQueryable<TurnDTO> GetTurnsDto();
    }
}
