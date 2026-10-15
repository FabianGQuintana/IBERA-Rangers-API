using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Application.DTOs.User
{
    // No incluye Password ni DNI: la contraseña se cambia por su propio endpoint y el DNI no cambia.
    public class UpdateUserDTO
    {
        public required string Name { get; set; }

        public required string LastName { get; set; }

        public required string PhoneNumber { get; set; }

        public required string Email { get; set; }

        public required IberaPortales Portal { get; set; }

        public required Guid IdRol { get; set; }
    }
}
