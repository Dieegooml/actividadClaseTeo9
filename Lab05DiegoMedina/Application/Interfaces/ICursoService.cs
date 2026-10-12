using Lab05DiegoMedina.Models;

namespace Lab05DiegoMedina.Services;

public interface ICursoService
{
    Task<List<Curso>> GetAllAsync();
    Task<Curso?> GetByIdAsync(int id);
    Task<Curso> CreateAsync(Curso entity);
    Task UpdateAsync(int id, Curso entity);
    Task DeleteAsync(int id);
}