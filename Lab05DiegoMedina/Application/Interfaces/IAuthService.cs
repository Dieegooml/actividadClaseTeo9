using Lab05DiegoMedina.Models;

namespace Lab05DiegoMedina.Services;

public record UsuarioInfo(int Id, string UserName, string Nombre, string? Correo, string Rol, bool Activo)
{
    public static UsuarioInfo FromEntity(Usuario usuario) =>
        new(usuario.IdUsuario, usuario.UserName, usuario.Nombre, usuario.Correo, usuario.Rol.ToString(), usuario.Activo);
}

public record AuthResult(string AccessToken, int ExpiresIn, UsuarioInfo Usuario);

public record ReporteInfo(int Estudiantes, int Profesores, int Cursos, int Usuarios);

public interface IAuthService
{
    Task<AuthResult?> LoginAsync(string userName, string password);

    Task<UsuarioInfo> RegisterAsync(string userName, string password, string? nombre, string? correo);
    Task<UsuarioInfo?> GetByIdAsync(int idUsuario);
    Task<List<UsuarioInfo>> GetAllAsync();
    Task<UsuarioInfo> CambiarRolAsync(int idUsuario, Rol rol, int idSolicitante);
    Task DeleteAsync(int idUsuario, int idSolicitante);
    Task<ReporteInfo> ObtenerReporteAsync();
}