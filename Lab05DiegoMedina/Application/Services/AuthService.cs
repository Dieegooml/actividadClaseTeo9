using Lab05DiegoMedina.Models;
using Lab05DiegoMedina.UnitOfWork;

namespace Lab05DiegoMedina.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResult?> LoginAsync(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            return null;

        var usuarios = await _unitOfWork.Repository<Usuario>().FindAsync(u => u.UserName == userName.Trim());
        var usuario = usuarios.FirstOrDefault();

        if (usuario is null || !_passwordHasher.Verify(password, usuario.Password))
            return null;

        if (!usuario.Activo)
            return null;

        var token = _tokenService.GenerarToken(usuario);

        return new AuthResult(token.Token, token.ExpiresInSeconds, UsuarioInfo.FromEntity(usuario));
    }

    public async Task<UsuarioInfo> RegisterAsync(string userName, string password, string? nombre, string? correo)
    {
        ValidarRegistro(userName, password, nombre);
        await EnsureUserNameDisponibleAsync(userName);

        var usuario = new Usuario
        {
            UserName = userName.Trim(),
            Password = _passwordHasher.Hash(password),
            Nombre = nombre!.Trim(),
            Correo = correo?.Trim(),
            Rol = Rol.Estudiante,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Usuario>().AddAsync(usuario);
        await _unitOfWork.Complete();

        return UsuarioInfo.FromEntity(usuario);
    }

    public async Task<UsuarioInfo?> GetByIdAsync(int idUsuario)
    {
        var usuario = await _unitOfWork.Repository<Usuario>().GetByIdAsync(idUsuario);
        return usuario is null ? null : UsuarioInfo.FromEntity(usuario);
    }

    public async Task<List<UsuarioInfo>> GetAllAsync()
    {
        var usuarios = await _unitOfWork.Repository<Usuario>().GetAllAsync();
        return usuarios.OrderBy(u => u.UserName).Select(UsuarioInfo.FromEntity).ToList();
    }

    public async Task<UsuarioInfo> CambiarRolAsync(int idUsuario, Rol rol, int idSolicitante)
    {
        var usuario = await ObtenerUsuarioAsync(idUsuario);

        if (usuario.IdUsuario == idSolicitante)
            throw new InvalidOperationException("No puede cambiar su propio rol.");

        if (usuario.Rol == Rol.Admin && rol != Rol.Admin && await EsUltimoAdminAsync())
            throw new InvalidOperationException("No se puede quitar el rol de administrador al último administrador.");

        usuario.Rol = rol;
        _unitOfWork.Repository<Usuario>().Update(usuario);
        await _unitOfWork.Complete();

        return UsuarioInfo.FromEntity(usuario);
    }

    public async Task DeleteAsync(int idUsuario, int idSolicitante)
    {
        var usuario = await ObtenerUsuarioAsync(idUsuario);

        if (usuario.IdUsuario == idSolicitante)
            throw new InvalidOperationException("No puede eliminar su propia cuenta.");

        if (usuario.Rol == Rol.Admin && await EsUltimoAdminAsync())
            throw new InvalidOperationException("No se puede eliminar al último administrador.");

        _unitOfWork.Repository<Usuario>().Delete(usuario);
        await _unitOfWork.Complete();
    }

    public async Task<ReporteInfo> ObtenerReporteAsync()
    {
        var estudiantes = await _unitOfWork.Repository<Estudiante>().GetAllAsync();
        var profesores = await _unitOfWork.Repository<Profesore>().GetAllAsync();
        var cursos = await _unitOfWork.Repository<Curso>().GetAllAsync();
        var usuarios = await _unitOfWork.Repository<Usuario>().GetAllAsync();

        return new ReporteInfo(estudiantes.Count, profesores.Count, cursos.Count, usuarios.Count);
    }

    private async Task<Usuario> ObtenerUsuarioAsync(int idUsuario)
    {
        return await _unitOfWork.Repository<Usuario>().GetByIdAsync(idUsuario)
               ?? throw new InvalidOperationException($"No existe el usuario con id {idUsuario}.");
    }

    private static void ValidarRegistro(string userName, string password, string? nombre)
    {
        if (string.IsNullOrWhiteSpace(userName) || userName.Trim().Length < 3)
            throw new InvalidOperationException("El usuario es obligatorio y debe tener al menos 3 caracteres.");

        if (string.IsNullOrEmpty(password) || password.Length < 8)
            throw new InvalidOperationException("La contraseña debe tener al menos 8 caracteres.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new InvalidOperationException("El nombre es obligatorio.");
    }

    private async Task EnsureUserNameDisponibleAsync(string userName)
    {
        var existentes = await _unitOfWork.Repository<Usuario>().FindAsync(u => u.UserName == userName.Trim());

        if (existentes.Any())
            throw new InvalidOperationException($"El nombre de usuario '{userName}' ya está registrado.");
    }

    private async Task<bool> EsUltimoAdminAsync()
    {
        var administradores = await _unitOfWork.Repository<Usuario>().FindAsync(u => u.Rol == Rol.Admin && u.Activo);
        return administradores.Count() <= 1;
    }
}