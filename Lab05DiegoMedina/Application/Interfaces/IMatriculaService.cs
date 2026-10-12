using Lab05DiegoMedina.Models;
 
namespace Lab05DiegoMedina.Services;
 
public interface IMatriculaService
{
    Task<List<Matricula>> GetAllAsync();
    Task<Matricula?> GetByIdAsync(int id);
 
    // Valida que estudiante y curso existan, y evita matrículas duplicadas.
    Task<Matricula> MatricularAsync(Matricula matricula);
 
    Task DeleteAsync(int id);
}