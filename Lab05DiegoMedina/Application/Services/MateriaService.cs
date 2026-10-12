using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.UnitOfWork;

namespace Lab05DiegoMedina.Services;

public class MateriaService : IMateriaService
{
    private readonly IUnitOfWork _unitOfWork;

    public MateriaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Materia>> GetAllAsync() =>
        await _unitOfWork.Repository<Materia>().GetAllAsync();

    public async Task<Materia?> GetByIdAsync(int id) =>
        await _unitOfWork.Repository<Materia>().GetByIdAsync(id);

    public async Task<Materia> CreateAsync(Materia entity)
    {
        if (await _unitOfWork.Repository<Materia>().ExistsAsync(entity.IdMateria))
            throw new InvalidOperationException($"Ya existe una materia con id {entity.IdMateria}.");

        await _unitOfWork.Repository<Materia>().AddAsync(entity);
        await _unitOfWork.Complete();
        return entity;
    }

    public async Task UpdateAsync(int id, Materia entity)
    {
        if (id != entity.IdMateria)
            throw new InvalidOperationException("El id de la ruta no coincide con el de la entidad.");

        if (!await _unitOfWork.Repository<Materia>().ExistsAsync(id))
            throw new KeyNotFoundException($"No existe la materia con id {id}.");

        _unitOfWork.Repository<Materia>().Update(entity);
        await _unitOfWork.Complete();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Repository<Materia>().GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"No existe la materia con id {id}.");

        _unitOfWork.Repository<Materia>().Delete(entity);
        await _unitOfWork.Complete();
    }
}