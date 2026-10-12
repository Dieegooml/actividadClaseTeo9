using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.UnitOfWork;

namespace Lab05DiegoMedina.Services;

public class EvaluacionService : IEvaluacionService
{
    private readonly IUnitOfWork _unitOfWork;

    public EvaluacionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Evaluacione>> GetAllAsync() =>
        await _unitOfWork.Repository<Evaluacione>().GetAllAsync();

    public async Task<Evaluacione?> GetByIdAsync(int id) =>
        await _unitOfWork.Repository<Evaluacione>().GetByIdAsync(id);

    public async Task<Evaluacione> CreateAsync(Evaluacione entity)
    {
        if (await _unitOfWork.Repository<Evaluacione>().ExistsAsync(entity.IdEvaluacion))
            throw new InvalidOperationException($"Ya existe una evaluación con id {entity.IdEvaluacion}.");

        await _unitOfWork.Repository<Evaluacione>().AddAsync(entity);
        await _unitOfWork.Complete();
        return entity;
    }

    public async Task UpdateAsync(int id, Evaluacione entity)
    {
        if (id != entity.IdEvaluacion)
            throw new InvalidOperationException("El id de la ruta no coincide con el de la entidad.");

        if (!await _unitOfWork.Repository<Evaluacione>().ExistsAsync(id))
            throw new KeyNotFoundException($"No existe la evaluación con id {id}.");

        _unitOfWork.Repository<Evaluacione>().Update(entity);
        await _unitOfWork.Complete();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Repository<Evaluacione>().GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"No existe la evaluación con id {id}.");

        _unitOfWork.Repository<Evaluacione>().Delete(entity);
        await _unitOfWork.Complete();
    }
}