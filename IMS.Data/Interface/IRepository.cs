using Microsoft.EntityFrameworkCore;

namespace IMS.Data.Interface
{
    public interface IRepository<T>
    {
        T Get<Tkey>(Tkey id);
        IQueryable<T> GetAll();
        Task<T> GetByIdAsync<Tkey>(Tkey id);
        EntityState Add(T entity);
        EntityState Update(T entity);
        EntityState Delete(T entity);
        Task AddRangeAsync(List<T> entities);

    }
}
