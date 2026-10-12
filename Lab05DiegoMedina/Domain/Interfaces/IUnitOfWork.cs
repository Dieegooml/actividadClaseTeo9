using Lab05DiegoMedina.Repositories;

namespace Lab05DiegoMedina.UnitOfWork;

public interface IUnitOfWork 
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> Complete();
}