using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab05DiegoMedina.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfesoresController : ControllerBase
{
    private readonly IProfesorService _service;

    public ProfesoresController(IProfesorService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "ProfesorOAdmin")]
    public async Task<ActionResult<List<Profesore>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ProfesorOAdmin")]
    public async Task<ActionResult<Profesore>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);

        if (entity is null) return NotFound($"No existe el profesor con id {id}.");
        return Ok(entity);
    }

    [HttpPost]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<ActionResult<Profesore>> Create([FromBody] Profesore entity)
    {
        try
        {
            var creado = await _service.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = creado.IdProfesor }, creado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] Profesore entity)
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