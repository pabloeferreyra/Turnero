namespace Turnero.SL.Services.Repositories
{
    public class TurnDTORepository(ApplicationDbContext context, IMapper mapper, IMemoryCache cache) : RepositoryBase<TurnDTO>(context, mapper, cache), ITurnDTORepository
    {
        private readonly IMapper mapper = mapper;

        public IQueryable<TurnDTO> GetListDto(string connectionString)
        {
            var turnDto = CallStoredProcedureDTO(connectionString, "select * from getallturns()");
            return turnDto;
        }
    }

    public interface ITurnDTORepository
    {
        IQueryable<TurnDTO> GetListDto(string connectionString);
    }
}
