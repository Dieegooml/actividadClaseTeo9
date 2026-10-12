using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.UnitOfWork;

namespace Lab05DiegoMedina.Services;

public class AsistenciaService : IAsistenciaService
{
    private readonly IUnitOfWork _unitOfWork;

    public AsistenciaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Asistencia>> GetAllAsync() =>
        await _unitOfWork.Repository<Asistencia>().GetAllAsync();

    public async Task<Asistencia?> GetByIdAsync(int id) =>
        await _unitOfWork.Repository<Asistencia>().GetByIdAsync(id);

    public async Task<Asistencia> CreateAsync(Asistencia entity)
    {
        if (await _unitOfWork.Repository<Asistencia>().ExistsAsync(entity.IdAsistencia))
            throw new InvalidOperationException($"Ya existe una asistencia con id {entity.IdAsistencia}.");

        await _unitOfWork.Repository<Asistencia>().AddAsync(entity);
        await _unitOfWork.Complete();
        return entity;
    }

    public async Task UpdateAsync(int id, Asistencia entity)
    {
        if (id != entity.IdAsistencia)
            throw new InvalidOperationException("El id de la ruta no coincide con el de la entidad.");

        if (!await _unitOfWork.Repository<Asistencia>().ExistsAsync(id))
            throw new KeyNotFoundException($"No existe la asistencia con id {id}.");

        _unitOfWork.Repository<Asistencia>().Update(entity);
        await _unitOfWork.Complete();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Repository<Asistencia>().GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"No existe la asistencia con id {id}.");

        _unitOfWork.Repository<Asistencia>().Delete(entity);
        await _unitOfWork.Complete();
    }
}