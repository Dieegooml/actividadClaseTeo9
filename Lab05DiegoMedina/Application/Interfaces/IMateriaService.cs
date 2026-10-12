using Lab05DiegoMedina.Models;

namespace Lab05DiegoMedina.Services;

public interface IMateriaService
{
    Task<List<Materia>> GetAllAsync();
    Task<Materia?> GetByIdAsync(int id);
    Task<Materia> CreateAsync(Materia entity);
    Task UpdateAsync(int id, Materia entity);
    Task DeleteAsync(int id);
}