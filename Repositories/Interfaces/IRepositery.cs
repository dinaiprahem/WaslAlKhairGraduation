using System.Linq.Expressions;

namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface IRepositery<T> where T : class
    {
        Task CreateAsync(T entity);
        Task RemoveAsync(T entity);
        Task<T> GetAsync(Expression<Func<T, bool>>? filter = null);
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter = null);
    }
}
