namespace Turnero.Services.Repositories
{
    public class TurnDTORepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<TurnDTO>(context, cache), ITurnDTORepository
    {
        public IQueryable<TurnDTO> GetListDto(string connectionString)
        {
            var turnDto = this.CallStoredProcedureDTO(connectionString, "select * from getallturns()");
            return turnDto;
        }
    }

    public interface ITurnDTORepository
    {
        IQueryable<TurnDTO> GetListDto(string connectionString);
    }
}
