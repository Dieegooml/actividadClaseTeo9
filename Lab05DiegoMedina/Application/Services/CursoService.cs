using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.UnitOfWork;

namespace Lab05DiegoMedina.Services;

public class CursoService : ICursoService
{
    private readonly IUnitOfWork _unitOfWork;

    public CursoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Curso>> GetAllAsync() =>
        await _unitOfWork.Repository<Curso>().GetAllAsync();

    public async Task<Curso?> GetByIdAsync(int id) =>
        await _unitOfWork.Repository<Curso>().GetByIdAsync(id);

    public async Task<Curso> CreateAsync(Curso entity)
    {
        if (await _unitOfWork.Repository<Curso>().ExistsAsync(entity.IdCurso))
            throw new InvalidOperationException($"Ya existe un curso con id {entity.IdCurso}.");

        await _unitOfWork.Repository<Curso>().AddAsync(entity);
        await _unitOfWork.Complete();
        return entity;
    }

    public async Task UpdateAsync(int id, Curso entity)
    {
        if (id != entity.IdCurso)
            throw new InvalidOperationException("El id de la ruta no coincide con el de la entidad.");

        if (!await _unitOfWork.Repository<Curso>().ExistsAsync(id))
            throw new KeyNotFoundException($"No existe el curso con id {id}.");

        _unitOfWork.Repository<Curso>().Update(entity);
        await _unitOfWork.Complete();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Repository<Curso>().GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"No existe el curso con id {id}.");

        _unitOfWork.Repository<Curso>().Delete(entity);
        await _unitOfWork.Complete();
    }
}