using System.Linq.Expressions;

namespace Lab05DiegoMedina.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    Task AddAsync(T entity);

    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(int id);
}