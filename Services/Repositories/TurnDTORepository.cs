using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using Turnero.Data;
using Turnero.Models;

namespace Turnero.Services.Repositories
{
    public class TurnDTORepository : RepositoryBase<TurnDTO>, ITurnDTORepository
    {
        private readonly IMapper mapper;

        public TurnDTORepository(ApplicationDbContext context, IMapper mapper, IMemoryCache cache) : base(context, mapper, cache)
        {
            this.mapper = mapper;
        }

        public IQueryable<TurnDTO> GetListDto(string connectionString)
        {
            var turnDto = this.CallStoredProcedureDTO(connectionString, "GetAllTurns");
            return turnDto;
        }
    }

    public interface ITurnDTORepository
    {
        IQueryable<TurnDTO> GetListDto(string connectionString);
    }
}
