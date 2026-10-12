using Lab05DiegoMedina.Models;

namespace Lab05DiegoMedina.Services;

public interface IEvaluacionService
{
    Task<List<Evaluacione>> GetAllAsync();
    Task<Evaluacione?> GetByIdAsync(int id);
    Task<Evaluacione> CreateAsync(Evaluacione entity);
    Task UpdateAsync(int id, Evaluacione entity);
    Task DeleteAsync(int id);
}