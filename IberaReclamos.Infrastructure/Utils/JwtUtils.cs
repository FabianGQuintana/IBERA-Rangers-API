using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IberaReclamos.Application.Common.Options;
using IberaReclamos.Application.Interfaces.Common;
using IberaReclamos.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IberaReclamos.Infrastructure.Utils
{
    // Generación y validación de tokens JWT.
    // Un JWT tiene 3 partes: header.payload.firma. El payload lleva los "claims" (id, email, rol)
    // y la firma (HMAC-SHA256 con nuestra Key secreta) garantiza que nadie los modificó.
    public class JwtUtils : ITokenService
    {
        public const string UserIdClaim = "id";

        private readonly JwtOptions _options;

        public JwtUtils(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public GeneratedToken GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new(UserIdClaim, user.ID.ToString()),
                new(JwtRegisteredClaimNames.Sub, user.ID.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("name", $"{user.Name} {user.LastName}"),
                // [Authorize(Roles = "Admin")] lee este claim
                new(ClaimTypes.Role, user.Rol.RolName.ToString()),
                new("portal", user.Portal.ToString()),
                // Id único del token (permite revocarlo en el futuro con una lista negra)
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expiresAt = DateTime.UtcNow.AddMinutes(_options.DurationInMinutes);
            var credentials = new SigningCredentials(BuildSigningKey(_options.Key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new GeneratedToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        // Valida firma, emisor, audiencia y vencimiento. Devuelve null si el token no sirve.
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
                var principal = handler.ValidateToken(token, BuildValidationParameters(_options), out var validatedToken);

                // Evita el ataque de "cambiar el algoritmo" del header (ej: alg = none)
                if (validatedToken is not JwtSecurityToken jwt
                    || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                    return null;

                return principal;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        // Las MISMAS reglas que usa el middleware de autenticación en Program.cs (una sola fuente de verdad)
        public static TokenValidationParameters BuildValidationParameters(JwtOptions options) => new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = options.Issuer,
            ValidAudience = options.Audience,
            IssuerSigningKey = BuildSigningKey(options.Key),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "name",
            ClockSkew = TimeSpan.Zero // sin tolerancia: cuando vence, vence
        };

        private static SymmetricSecurityKey BuildSigningKey(string key) => new(Encoding.UTF8.GetBytes(key));
    }
}
