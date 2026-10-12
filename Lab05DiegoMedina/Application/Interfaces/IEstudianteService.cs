using Lab05DiegoMedina.Models;

namespace Lab05DiegoMedina.Services;

public interface IEstudianteService
{
    Task<List<Estudiante>> GetAllAsync();
    Task<Estudiante?> GetByIdAsync(int id);
    Task<Estudiante> CreateAsync(Estudiante entity);
    Task UpdateAsync(int id, Estudiante entity);
    Task DeleteAsync(int id);
}