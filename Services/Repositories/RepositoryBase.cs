namespace Turnero.Services.Repositories;

public abstract class RepositoryBase<T>(ApplicationDbContext context, IMemoryCache cache) : IRepositoryBase<T> where T : class
{
    protected ApplicationDbContext _context = context;
    public IMemoryCache _cache = cache;

    public IQueryable<T> FindAll() => this._context.Set<T>().AsNoTracking();
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        return this._context.Set<T>().Where(expression).AsNoTracking();
    }

    public void Create(T entity)
    {
        this._context.Set<T>().Add(entity);
        this._context.SaveChanges();
    }

    public async Task CreateAsync(T entity)
    {
        this._context.Set<T>().Add(entity);
        await this._context.SaveChangesAsync();
    }

    public void Update(T entity)
    {
        this._context.Set<T>().Update(entity);
        this._context.SaveChanges();
    }
    public void Delete(T entity)
    {
        this._context.Set<T>().Remove(entity);
        this._context.SaveChanges();
    }

    public async Task UpdateAsync(T entity)
    {
        this._context.Set<T>().Update(entity);
        await this._context.SaveChangesAsync();
    }
    public void DeleteAsync(T entity)
    {
        this._context.Set<T>().Remove(entity);
        this._context.SaveChanges();
    }

    public async Task<List<TResult>> GetCachedData<TResult>(string cacheKey, Func<Task<List<TResult>>> getDataFunc)
    {
        var data = _cache.Get<List<TResult>>(cacheKey);
        if (data.IsNullOrEmpty())
        {
            data = await getDataFunc();
            _cache.Set(cacheKey, data);
        }
        return data;
    }

    public List<T> CallStoredProcedure(string procedureName, params object[] parameters)
    {
        var sqlParameters = new List<SqlParameter>();
        var sqlParametersString = new StringBuilder();

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameterName = $"@p{i}";
            var sqlParameter = new SqlParameter(parameterName, parameters[i]);
            sqlParameters.Add(sqlParameter);
            sqlParametersString.Append(parameters[i]);

            if (i != parameters.Length - 1)
            {
                sqlParametersString.Append(", ");
            }
        }

        var sql = $"select * from {procedureName}({sqlParametersString})";

        return [.. this._context.Set<T>().FromSqlRaw(sql)];
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

    private static List<T> MapResults(NpgsqlDataReader reader)
    {
        var results = new List<T>();
        var properties = typeof(T).GetProperties();

        while (reader.Read())
        {
            var instance = Activator.CreateInstance<T>();

            foreach (var property in properties)
            {
                if (reader[property.Name] != DBNull.Value)
                {
                    property.SetValue(instance, reader[property.Name]);
                }
            }

            results.Add(instance);
        }

        return results;
    }
}