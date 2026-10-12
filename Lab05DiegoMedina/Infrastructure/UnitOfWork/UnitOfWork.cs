using System.Collections;
using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.Repositories;

namespace Lab05DiegoMedina.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly Hashtable _repositories = new();
    private readonly ColegioDBContext _context;

    public UnitOfWork(ColegioDBContext context)
    {
        _context = context;
    }

    public Task<int> Complete()
    {
        return _context.SaveChangesAsync();
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity).Name;

        if (_repositories[type] is IGenericRepository<TEntity> existente)
            return existente;

        var tipoRepositorio = typeof(GenericRepository<>).MakeGenericType(typeof(TEntity));
        var repositorio = (IGenericRepository<TEntity>)Activator.CreateInstance(tipoRepositorio, _context)!;

        _repositories.Add(type, repositorio);
        return repositorio;
    }
}
