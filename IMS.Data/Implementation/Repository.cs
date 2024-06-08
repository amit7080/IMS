using IMS.Data.Context;
using IMS.Data.Interface;
using Microsoft.EntityFrameworkCore;

namespace IMS.Data.Implementation
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly IMSDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(IMSDbContext dbcontext)
        {
            _context = dbcontext;
            _dbSet = _context.Set<T>();
        }

        public T Get<Tkey>(Tkey id)
        {
            return _dbSet.Find(id);
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<T> GetByIdAsync<Tkey>(Tkey id)
        {
            return await _dbSet.FindAsync(id);
        }

        public EntityState Add(T entity)
        {
            return _dbSet.Add(entity).State;
        }
        public async Task AddRangeAsync(List<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }
        public EntityState Update(T entity)
        {
            return _dbSet.Update(entity).State;
        }


        public EntityState Delete(T entity)
        {
            return _dbSet.Remove(entity).State;
        }
    }
}
