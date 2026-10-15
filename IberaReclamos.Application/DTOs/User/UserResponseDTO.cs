namespace IberaReclamos.Application.DTOs.User
{
    // Lo que devolvemos al frontend: nunca incluye el hash de la contraseña.
    public class UserResponseDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Dni { get; set; } = string.Empty;

        public string Portal { get; set; } = string.Empty;

        public Guid IdRol { get; set; }

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreateAt { get; set; }
    }
}
