using System.Globalization;
using System.Security.Claims;
using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab05DiegoMedina.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Login([FromQuery] string userName, [FromQuery] string password)
    {
        var response = await _authService.LoginAsync(userName, password);

        if (response is null)
            return Unauthorized("Credenciales inválidas");

        return Ok(response);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UsuarioInfo>> Register([FromQuery] string userName, [FromQuery] string password, [FromQuery] string? nombre, [FromQuery] string? correo)
    {
        try
        {
            var user = await _authService.RegisterAsync(userName, password, nombre, correo);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet("usuarios")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<ActionResult<List<UsuarioInfo>>> GetUsuarios()
    {
        return Ok(await _authService.GetAllAsync());
    }

    [HttpGet("usuarios/{id:int}")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<ActionResult<UsuarioInfo>> GetById(int id)
    {
        var user = await _authService.GetByIdAsync(id);

        if (user is null) return NotFound($"No existe el usuario con id {id}.");
        return Ok(user);
    }

    [HttpPut("usuarios/{id:int}/rol")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<ActionResult<UsuarioInfo>> CambiarRol(int id, [FromQuery] Rol rol)
    {
        if (await _authService.GetByIdAsync(id) is null)
            return NotFound($"No existe el usuario con id {id}.");

        try
        {
            return Ok(await _authService.CambiarRolAsync(id, rol, IdUsuarioActual()));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("usuarios/{id:int}")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _authService.GetByIdAsync(id) is null)
            return NotFound($"No existe el usuario con id {id}");

        try
        {
            await _authService.DeleteAsync(id, IdUsuarioActual());
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("admin/reporte")]
    [Authorize(Policy = "SoloAdmin")]
    public async Task<IActionResult> Reporte()
    {
        return Ok(await _authService.ObtenerReporteAsync());
    }

    private int IdUsuarioActual()
    {
        var id = User.FindFirstValue(JwtTokenService.IdClaim);

        if (!int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idUsuario))
            throw new InvalidOperationException("El token no contiene un usuario válido");

        return idUsuario;
    }
}