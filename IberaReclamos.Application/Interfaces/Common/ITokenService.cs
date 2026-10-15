using IberaReclamos.Domain.Entities;

namespace IberaReclamos.Application.Interfaces.Common
{
    public sealed record GeneratedToken(string Token, DateTime ExpiresAt);

    // Contrato para emitir el JWT del login. La implementación (JwtUtils) vive en Infrastructure/Utils.
    public interface ITokenService
    {
        // El usuario debe venir con el Rol cargado (Include) para poner el rol en el token.
        GeneratedToken GenerateToken(User user);
    }
}
