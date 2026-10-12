using Lab05DiegoMedina.Models;

namespace Lab05DiegoMedina.Services;

public interface IAsistenciaService
{
    Task<List<Asistencia>> GetAllAsync();
    Task<Asistencia?> GetByIdAsync(int id);
    Task<Asistencia> CreateAsync(Asistencia entity);
    Task UpdateAsync(int id, Asistencia entity);
    Task DeleteAsync(int id);
}