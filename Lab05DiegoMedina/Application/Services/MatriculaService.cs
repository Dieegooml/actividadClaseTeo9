using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.UnitOfWork;

namespace Lab05DiegoMedina.Services;

public class MatriculaService : IMatriculaService
{
    private readonly IUnitOfWork _unitOfWork;

    public MatriculaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Matricula>> GetAllAsync() =>
        await _unitOfWork.Repository<Matricula>().GetAllAsync();

    public async Task<Matricula?> GetByIdAsync(int id) =>
        await _unitOfWork.Repository<Matricula>().GetByIdAsync(id);

    public async Task<Matricula> MatricularAsync(Matricula matricula)
    {
        if (matricula.IdEstudiante == null)
            throw new InvalidOperationException("Debe indicar el id del estudiante.");

        if (matricula.IdCurso == null)
            throw new InvalidOperationException("Debe indicar el id del curso.");

        var estudiante = await _unitOfWork.Repository<Estudiante>().GetByIdAsync(matricula.IdEstudiante.Value);
        if (estudiante == null)
            throw new InvalidOperationException($"No existe el estudiante con id {matricula.IdEstudiante}.");

        var curso = await _unitOfWork.Repository<Curso>().GetByIdAsync(matricula.IdCurso.Value);
        if (curso == null)
            throw new InvalidOperationException($"No existe el curso con id {matricula.IdCurso}.");

        var yaMatriculado = await _unitOfWork.Repository<Matricula>().FindAsync(m =>
            m.IdEstudiante == matricula.IdEstudiante &&
            m.IdCurso == matricula.IdCurso &&
            m.Semestre == matricula.Semestre);

        if (yaMatriculado.Any())
            throw new InvalidOperationException("El estudiante ya está matriculado en este curso para este semestre.");

        await _unitOfWork.Repository<Matricula>().AddAsync(matricula);
        await _unitOfWork.Complete();
        return matricula;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Repository<Matricula>().GetByIdAsync(id);
        if (entity != null)
        {
            _unitOfWork.Repository<Matricula>().Delete(entity);
            await _unitOfWork.Complete();
        }
    }
}