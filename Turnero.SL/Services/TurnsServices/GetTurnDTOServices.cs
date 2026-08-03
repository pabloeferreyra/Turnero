namespace Turnero.SL.Services.TurnsServices;

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

    public IQueryable<TurnDTO> GetTurnsDtoByDateAndId(DateOnly date, Guid? id)
    {
        try
        {
            var medicSuffix = id.HasValue ? id.Value.ToString()[..8] : "all";
            // Miss: fetch from database via stored procedure
            var data = turnRepository.GetListDtoParam(_connectionString, date, id);
            var dataList = data.ToList();


            return dataList.AsQueryable();
        }
        catch (InvalidOperationException ex)
        {
            throw new ApplicationException("Error al obtener TurnDTOs por fecha e ID.", ex);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error inesperado al obtener TurnDTOs por fecha e ID.", ex);
        }
    }
}

public interface IGetTurnDTOServices
{
    IQueryable<TurnDTO> GetTurnsDto();
    IQueryable<TurnDTO> GetTurnsDtoByDateAndId(DateOnly date, Guid? id);
}
