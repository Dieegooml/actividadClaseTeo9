using System.Linq.Expressions;
using Lab05DiegoMedina.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab05DiegoMedina.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ColegioDBContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ColegioDBContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        // No se llama a SaveChangesAsync: el commit lo hace UnitOfWork.Complete()
    }

    public void Update(T entity)
    {
        _context.ChangeTracker.Clear();

        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        _dbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(int id) => await _dbSet.FindAsync(id) != null;
}