using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.UnitOfWork;

namespace Lab05DiegoMedina.Services;

public class ProfesorService : IProfesorService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProfesorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Profesore>> GetAllAsync() =>
        await _unitOfWork.Repository<Profesore>().GetAllAsync();

    public async Task<Profesore?> GetByIdAsync(int id) =>
        await _unitOfWork.Repository<Profesore>().GetByIdAsync(id);

    public async Task<Profesore> CreateAsync(Profesore entity)
    {
        if (await _unitOfWork.Repository<Profesore>().ExistsAsync(entity.IdProfesor))
            throw new InvalidOperationException($"Ya existe un profesor con id {entity.IdProfesor}.");

        await _unitOfWork.Repository<Profesore>().AddAsync(entity);
        await _unitOfWork.Complete();
        return entity;
    }

    public async Task UpdateAsync(int id, Profesore entity)
    {
        if (id != entity.IdProfesor)
            throw new InvalidOperationException("El id de la ruta no coincide con el de la entidad.");

        if (!await _unitOfWork.Repository<Profesore>().ExistsAsync(id))
            throw new KeyNotFoundException($"No existe el profesor con id {id}.");

        _unitOfWork.Repository<Profesore>().Update(entity);
        await _unitOfWork.Complete();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Repository<Profesore>().GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"No existe el profesor con id {id}.");

        _unitOfWork.Repository<Profesore>().Delete(entity);
        await _unitOfWork.Complete();
    }
}