using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Turnero.Data;

namespace Turnero.Services.Repositories;

public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected ApplicationDbContext _context;
    private readonly IMapper mapper;
    public IMemoryCache _cache;

    public RepositoryBase(ApplicationDbContext context, IMapper mapper, IMemoryCache cache)
    {
        this._context = context;
        this.mapper = mapper;
        this._cache = cache;
    }

    public IQueryable<T> FindAll() => this._context.Set<T>().AsNoTracking();
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression) => this._context.Set<T>().Where(expression).AsNoTracking();

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
}