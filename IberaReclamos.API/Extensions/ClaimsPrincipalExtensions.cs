using System.Security.Claims;
using IberaReclamos.Domain.ConstantsRoles;
using IberaReclamos.Domain.Exceptions;
using IberaReclamos.Infrastructure.Utils;

namespace IberaReclamos.API.Extensions
{
    // Atajos para leer los datos del usuario logueado desde el token (User en un controller).
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(JwtUtils.UserIdClaim);
            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedException("El token no tiene un usuario válido.");
        }

        public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(RoleNames.Admin);
    }
}
