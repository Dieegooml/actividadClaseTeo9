using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.UnitOfWork;

namespace Lab05DiegoMedina.Services;

public class EstudianteService : IEstudianteService
{
    private readonly IUnitOfWork _unitOfWork;

    public EstudianteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Estudiante>> GetAllAsync() =>
        await _unitOfWork.Repository<Estudiante>().GetAllAsync();

    public async Task<Estudiante?> GetByIdAsync(int id) =>
        await _unitOfWork.Repository<Estudiante>().GetByIdAsync(id);

    public async Task<Estudiante> CreateAsync(Estudiante entity)
    {
        if (await _unitOfWork.Repository<Estudiante>().ExistsAsync(entity.IdEstudiante))
            throw new InvalidOperationException($"Ya existe un estudiante con id {entity.IdEstudiante}.");

        await _unitOfWork.Repository<Estudiante>().AddAsync(entity);
        await _unitOfWork.Complete();
        return entity;
    }

    public async Task UpdateAsync(int id, Estudiante entity)
    {
        if (id != entity.IdEstudiante)
            throw new InvalidOperationException("El id de la ruta no coincide con el de la entidad.");

        if (!await _unitOfWork.Repository<Estudiante>().ExistsAsync(id))
            throw new KeyNotFoundException($"No existe el estudiante con id {id}.");

        _unitOfWork.Repository<Estudiante>().Update(entity);
        await _unitOfWork.Complete();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Repository<Estudiante>().GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"No existe el estudiante con id {id}.");

        _unitOfWork.Repository<Estudiante>().Delete(entity);
        await _unitOfWork.Complete();
    }
}