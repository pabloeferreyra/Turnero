namespace Turnero.Infrastructure.Repositories
{
    public class TurnDTORepository(ApplicationDbContext context, IMemoryCache cache, IOptions<DatabaseOptions> databaseOptions) : RepositoryBase<TurnDTO>(context, cache, databaseOptions), ITurnDTORepository
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

                var list = ExecuteGetTurns(connectionString, "select * from getturns(@p0, @p1)", p0, p1);
                return list.AsQueryable();
            }
            else
            {
                var p0 = new NpgsqlParameter("p0", date);
                var list = ExecuteGetTurns(connectionString, "select * from getturns(@p0)", p0);
                return list.AsQueryable();
            }
        }

        private List<TurnDTO> ExecuteGetTurns(string connectionString, string sql, params NpgsqlParameter[] parameters)
        {
            var results = new List<TurnDTO>();

            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            using var command = new NpgsqlCommand(sql, connection)
            {
                CommandType = CommandType.Text
            };

            if (parameters != null && parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            using var reader = command.ExecuteReader();

            // Build a case-insensitive set of column names present in the result set.
            var columnNames = new HashSet<string>(
                Enumerable.Range(0, reader.FieldCount).Select(reader.GetName),
                StringComparer.OrdinalIgnoreCase);

            while (reader.Read())
            {
                var dto = new TurnDTO();

                Guid TryGetGuid(string name)
                {
                    try
                    {
                        if (!columnNames.Contains(name) || reader[name] == DBNull.Value) return Guid.Empty;
                        return reader.GetGuid(reader.GetOrdinal(name));
                    }
                    catch
                    {
                        try { return Guid.Parse(reader[name].ToString()); } catch { return Guid.Empty; }
                    }
                }

                string? TryGetString(string name)
                {
                    try
                    {
                        if (!columnNames.Contains(name) || reader[name] == DBNull.Value) return null;
                        return reader[name].ToString();
                    }
                    catch { return null; }
                }

                long? TryGetLong(string name)
                {
                    try
                    {
                        if (!columnNames.Contains(name) || reader[name] == DBNull.Value) return null;
                        return Convert.ToInt64(reader[name]);
                    }
                    catch { return null; }
                }

                bool TryGetBool(string name)
                {
                    try
                    {
                        if (!columnNames.Contains(name) || reader[name] == DBNull.Value) return false;
                        return Convert.ToBoolean(reader[name]);
                    }
                    catch { return false; }
                }

                // Map commonly expected columns (defensive, case-insensitive)
                dto.Id = TryGetGuid("Id");
                dto.Name = TryGetString("Name");
                dto.Dni = TryGetLong("Dni");
                dto.MedicId = TryGetGuid("MedicId");
                dto.MedicName = TryGetString("MedicName");

                // Date may come as date/datetime/string — normalize to yyyy-MM-dd
                if (columnNames.Contains("Date") && reader["Date"] != DBNull.Value)
                {
                    try
                    {
                        var dateVal = reader["Date"];
                        if (dateVal is DateTime dt) dto.Date = dt.ToString("yyyy-MM-dd");
                        else dto.Date = dateVal.ToString();
                    }
                    catch { dto.Date = TryGetString("Date"); }
                }

                // Time: ensure string presentation (e.g., "10:00")
                if (columnNames.Contains("Time") && reader["Time"] != DBNull.Value)
                {
                    try
                    {
                        var timeVal = reader["Time"];
                        dto.Time = timeVal is TimeSpan ts ? ts.ToString(@"hh\:mm") : timeVal.ToString();
                    }
                    catch { dto.Time = TryGetString("Time"); }
                }

                dto.TimeId = TryGetGuid("TimeId");
                dto.SocialWork = TryGetString("SocialWork");
                dto.Reason = TryGetString("Reason");
                dto.Accessed = TryGetBool("Accessed");

                results.Add(dto);
            }

            return results;
        }
    }

}
