namespace Turnero.SL.Services.Repositories
{
    public class TurnDTORepository(ApplicationDbContext context, IMemoryCache cache) : RepositoryBase<TurnDTO>(context, cache), ITurnDTORepository
    {
        public IQueryable<TurnDTO> GetListDto(string connectionString)
        {
            var turnDto = CallStoredProcedureDTO(connectionString, "select * from getallturns()");
            return turnDto;
        }
        public IQueryable<TurnDTO> GetListDtoParam(string connectionString, DateOnly date, Guid? id)
        {
            if (id != null)
            {
                var p0 = new NpgsqlParameter("p0", date);
                var p1 = new NpgsqlParameter("p1", id);

                return CallStoredProcedureDTO(
                    connectionString,
                    "select * from getturns(@p0, @p1)",
                    p0, p1
                );
            } 
            else
            {
                var p0 = new NpgsqlParameter("p0", date);

                return CallStoredProcedureDTO(
                    connectionString,
                    "select * from getturns(@p0)",
                    p0
                );
            }
        }
    }

    public interface ITurnDTORepository
    {
        IQueryable<TurnDTO> GetListDto(string connectionString);
        IQueryable<TurnDTO> GetListDtoParam(string connectionString, DateOnly date, Guid? id);
    }
}
