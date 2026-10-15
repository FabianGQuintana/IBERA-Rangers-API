namespace IberaReclamos.Application.DTOs.Role
{
    public class RoleResponseDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
