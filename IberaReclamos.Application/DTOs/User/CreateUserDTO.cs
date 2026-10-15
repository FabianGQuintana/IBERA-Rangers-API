using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Application.DTOs.User
{
    public class CreateUserDTO
    {
        public required string Name { get; set; }

        public required string LastName { get; set; }

        public required string PhoneNumber { get; set; }

        public required string Email { get; set; }

        public required string Dni { get; set; }

        public required IberaPortales Portal { get; set; }

        // IMPORTANTE: Aquí pedimos la contraseña en "texto plano".
        // Nunca pidas el "Password_hash" en el DTO porque el Frontend no debe hashear la clave,
        // eso lo hace el Backend (en el UserService) usando BCrypt.
        public required string Password { get; set; }

        public required Guid IdRol { get; set; }
    }
}
