using IMS.Data.Context;
using IMS.Data.Interface;

namespace IMS.Data.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IMSDbContext _dbcontext;
        private Dictionary<Type, object> repositories;

        public UnitOfWork(IMSDbContext context)
        {
            _dbcontext = context;
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            repositories ??= new Dictionary<Type, object>();
            var type = typeof(TEntity);

            if (!repositories.ContainsKey(type))
            {
                repositories[type] = new Repository<TEntity>(_dbcontext);
            }
            return (IRepository<TEntity>)repositories[type];
        }
        public int commit()
        {
            return _dbcontext.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(obj: this);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dbcontext != null)
                {
                    _dbcontext.Dispose();
                }
            }
        }
    }
}
