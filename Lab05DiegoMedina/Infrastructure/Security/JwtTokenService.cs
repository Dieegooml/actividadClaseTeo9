using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lab05DiegoMedina.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace Lab05DiegoMedina.Services;

public class JwtTokenService : ITokenService
{
    public const string UserNameClaim = "unique_name";
    public const string NameClaim = "name";
    public const string EmailClaim = "email";
    public const string RoleClaim = "role";
    public const string IdClaim = JwtRegisteredClaimNames.Sub;

    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenResult GenerarToken(Usuario usuario)
    {
        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Falta la configuración 'Jwt:Issuer'.");
        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Falta la configuración 'Jwt:Audience'.");
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta la configuración 'Jwt:Key'.");

        var valorMinutos = _configuration["Jwt:ExpirationMinutes"];
        var minutos = int.TryParse(valorMinutos, out var m) ? m : 60;

        var expiracion = DateTime.UtcNow.AddMinutes(minutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(IdClaim, usuario.IdUsuario.ToString(CultureInfo.InvariantCulture)),
            new(UserNameClaim, usuario.UserName),
            new(NameClaim, usuario.Nombre),
            new(EmailClaim, usuario.Correo ?? string.Empty),
            new(RoleClaim, usuario.Rol.ToString()),
            new(
                JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64)
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiracion,
            signingCredentials: credenciales);

        return new TokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            minutos * 60);
    }
}