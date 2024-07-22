namespace Turnero.Services.Repositories;

public interface IRepositoryBase<T>
{
    IQueryable<T> FindAll();
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);

    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    void DeleteAsync(T entity);
    Task<List<TResult>> GetCachedData<TResult>(string cacheKey, Func<Task<List<TResult>>> getDataFunc);
    List<T> CallStoredProcedure(string procedureName, params object[] parameters);
    IQueryable<T> CallStoredProcedureDTO(string connectionString, string procedureName);
}