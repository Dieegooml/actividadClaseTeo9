using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab05DiegoMedina.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AsistenciasController : ControllerBase
{
    private readonly IAsistenciaService _service;

    public AsistenciasController(IAsistenciaService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "ProfesorOAdmin")]
    public async Task<ActionResult<List<Asistencia>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ProfesorOAdmin")]
    public async Task<ActionResult<Asistencia>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);

        if (entity is null) return NotFound($"No existe la asistencia con id {id}.");
        return Ok(entity);
    }

    [HttpPost]
    [Authorize(Policy = "ProfesorOAdmin")]
    public async Task<ActionResult<Asistencia>> Create([FromBody] Asistencia entity)
    {
        try
        {
            var creado = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = creado.IdAsistencia }, creado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ProfesorOAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] Asistencia entity)
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