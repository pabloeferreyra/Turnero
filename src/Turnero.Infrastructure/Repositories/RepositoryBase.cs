namespace Turnero.Infrastructure.Repositories;

/// <summary>
/// Repositorio base (EF Core + ADO.NET para stored procedures).
/// La connection string se inyecta via IOptions (sin estado estático global).
/// </summary>
public abstract class RepositoryBase<T>(ApplicationDbContext context, IMemoryCache cache, IOptions<DatabaseOptions> databaseOptions) : IRepositoryBase<T> where T : class
{
    protected ApplicationDbContext _context = context;
    public IMemoryCache _cache = cache;
    private readonly string _connectionString = databaseOptions.Value.PostgresConnection;

    public IQueryable<T> FindAll()
    {
        return _context.Set<T>().AsNoTracking();
    }

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression).AsNoTracking();
    }

    public void Create(T entity)
    {
        _context.Set<T>().Add(entity);
        _context.SaveChanges();
    }

    public async Task CreateAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
        _context.SaveChanges();
    }
    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
        _context.SaveChanges();
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public List<T> CallStoredProcedure(string procedureName, params object[] parameters)
    {
        var sqlParameters = new List<NpgsqlParameter>();
        var sqlParametersString = new StringBuilder();

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameterName = $"@p{i}";
            var value = parameters[i];
            var sqlParameter = new NpgsqlParameter(parameterName, value);

            // Force DateTime values to be sent as PostgreSQL 'date' type instead of 'timestamp'
            // to match the stored procedure signatures (e.g. GetTurns(date)).
            if (value is DateTime)
            {
                sqlParameter.NpgsqlDbType = NpgsqlDbType.Date;
            }

            sqlParameters.Add(sqlParameter);
            sqlParametersString.Append(parameterName);

            if (i != parameters.Length - 1)
            {
                sqlParametersString.Append(", ");
            }
        }

        var sql = $"select * from {procedureName}({sqlParametersString})";

        // Use ADO.NET + MapResults instead of FromSqlRaw to avoid EF Core's strict
        // column-name matching. Stored procedures may return columns with casing
        // (e.g. lowercase "dateturn") that doesn't match entity property names
        // (e.g. "DateTurn"), causing InvalidOperationException.
        var connectionString = _connectionString
            ?? throw new InvalidOperationException("ConnectionString is not configured.");

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        using var command = new NpgsqlCommand(sql, connection);
        if (sqlParameters.Count > 0)
            command.Parameters.AddRange(sqlParameters.ToArray());

        using var reader = command.ExecuteReader();
        return MapResults(reader);
    }

    public IQueryable<T> CallStoredProcedureDTO(string connectionString, string procedureName)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        var command = new NpgsqlCommand(procedureName, connection)
        {
            CommandType = CommandType.Text
        };

        var results = command.ExecuteReader();
        var mappedResults = MapResults(results);
        return mappedResults.AsQueryable();
    }

    public IQueryable<T> CallStoredProcedureDTO(
    string connectionString,
    string procedureName,
    params NpgsqlParameter[] parameters)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        var command = new NpgsqlCommand(procedureName, connection)
        {
            CommandType = CommandType.Text
        };

        if (parameters != null && parameters.Length > 0)
            command.Parameters.AddRange(parameters);

        var results = command.ExecuteReader();
        var mappedResults = MapResults(results);
        return mappedResults.AsQueryable();
    }


    private static List<T> MapResults(NpgsqlDataReader reader)
    {
        var results = new List<T>();
        var properties = typeof(T).GetProperties();

        // Build a case-insensitive set of column names present in the result set.
        // This prevents IndexOutOfRangeException when T has navigation properties
        // (e.g. Medic, Time on Turn) that don't exist as columns in the SP result.
        var columnNames = new HashSet<string>(
            Enumerable.Range(0, reader.FieldCount).Select(reader.GetName),
            StringComparer.OrdinalIgnoreCase);

        while (reader.Read())
        {
            var instance = Activator.CreateInstance<T>();

            foreach (var property in properties)
            {
                if (!columnNames.Contains(property.Name) || reader[property.Name] == DBNull.Value)
                    continue;

                var value = reader[property.Name];
                var propType = property.PropertyType;
                var targetType = Nullable.GetUnderlyingType(propType) ?? propType;

                if (property.Name is "Medic" or "Time" or "Turns")
                {
                    continue;
                }

                // If types are compatible, assign directly
                if (targetType.IsAssignableFrom(value.GetType()))
                {
                    property.SetValue(instance, value);
                    continue;
                }

                // If target is a complex reference (e.g. navigation property like TimeTurn)
                // and the reader returned a scalar (e.g. the Time string), skip assignment
                if (targetType.IsClass && targetType != typeof(string))
                {
                    continue;
                }

                // Try to convert simple types (int, Guid, DateTime, string, etc.)
                try
                {
                    var converted = Convert.ChangeType(value, targetType);
                    property.SetValue(instance, converted);
                }
                catch
                {
                    // If conversion fails, skip assigning this property
                    continue;
                }
            }

            results.Add(instance);
        }

        return results;
    }
}
