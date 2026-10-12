using Lab05DiegoMedina.Models;

namespace Lab05DiegoMedina.Services;

public interface IProfesorService
{
    Task<List<Profesore>> GetAllAsync();
    Task<Profesore?> GetByIdAsync(int id);
    Task<Profesore> CreateAsync(Profesore entity);
    Task UpdateAsync(int id, Profesore entity);
    Task DeleteAsync(int id);
}