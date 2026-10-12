using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab05DiegoMedina.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EstudiantesController : ControllerBase
{
    private readonly IEstudianteService _service;

    public EstudiantesController(IEstudianteService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "ProfesorOAdmin")]
    public async Task<ActionResult<List<Estudiante>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Estudiante>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);

        if (entity is null) return NotFound($"No existe el estudiante con id {id}.");
        return Ok(entity);
    }

    [HttpPost]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<ActionResult<Estudiante>> Create([FromBody] Estudiante entity)
    {
        try
        {
            var creado = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = creado.IdEstudiante }, creado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] Estudiante entity)
    {
        try
        {
            await _service.UpdateAsync(id, entity);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}