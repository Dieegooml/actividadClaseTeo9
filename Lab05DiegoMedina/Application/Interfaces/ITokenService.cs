using Lab05DiegoMedina.Models;

namespace Lab05DiegoMedina.Services;

public record TokenResult(string Token, int ExpiresInSeconds);

public interface ITokenService
{
    TokenResult GenerarToken(Usuario usuario);
}
