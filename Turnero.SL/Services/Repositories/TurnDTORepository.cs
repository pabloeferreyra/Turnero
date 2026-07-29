namespace Turnero.SL.Services.Repositories
{
    public class TurnDTORepository(ApplicationDbContext context, IMemoryCache cache, RedisCacheService redisCache) : RepositoryBase<TurnDTO>(context, cache, redisCache), ITurnDTORepository
    {
        /// <summary>
        /// TurnDTO no es una entidad mapeada en ApplicationDbContext.
        /// Este repositorio solo debe usar CallStoredProcedureDTO (que no pasa por EF Core).
        /// Los métodos heredados de RepositoryBase (FindAll, FindByCondition, etc.)
        /// lanzarán una excepción si se invocan porque TurnDTO no está registrado como DbSet.
        /// </summary>
        public new IQueryable<TurnDTO> FindAll()
            => throw new NotSupportedException("TurnDTO no es una entidad del contexto. Use CallStoredProcedureDTO en su lugar.");

        public new IQueryable<TurnDTO> FindByCondition(Expression<Func<TurnDTO, bool>> expression)
            => throw new NotSupportedException("TurnDTO no es una entidad del contexto. Use CallStoredProcedureDTO en su lugar.");

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
