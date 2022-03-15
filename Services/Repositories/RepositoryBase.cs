using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Turnero.Data;

namespace Turnero.Services.Repositories
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected ApplicationDbContext _context;
        public RepositoryBase(ApplicationDbContext context)
        {
            this._context = context;
        }
        public IQueryable<T> FindAll()
        {
            return this._context.Set<T>().AsNoTracking();
        }
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
        public async Task DeleteAsync(T entity)
        {
            this._context.Set<T>().Remove(entity);
            await this._context.SaveChangesAsync();
        }
    }
}