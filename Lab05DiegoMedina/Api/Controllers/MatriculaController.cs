using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab05DiegoMedina.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatriculasController : ControllerBase
{
    private readonly IMatriculaService _service;

    public MatriculasController(IMatriculaService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "ProfesorOAdmin")]
    public async Task<ActionResult<List<Matricula>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "EstudianteOAdmin")]
    public async Task<ActionResult<Matricula>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);

        if (entity is null) return NotFound($"No existe la matrícula con id {id}.");
        return Ok(entity);
    }

    [HttpPost]
    [Authorize(Policy = "ProfesorOAdmin")]
    public async Task<ActionResult<Matricula>> Create([FromBody] Matricula entity)
    {
        try
        {
            var creado = await _service.MatricularAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = creado.IdMatricula }, creado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _service.GetByIdAsync(id) is null)
            return NotFound($"No existe la matrícula con id {id}.");

        await _service.DeleteAsync(id);
        return NoContent();
    }
}
