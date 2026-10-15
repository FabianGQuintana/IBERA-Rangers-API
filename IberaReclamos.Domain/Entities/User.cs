using IberaReclamos.Domain.Enum;


namespace IberaReclamos.Domain.Entities
{
    public class User : BaseEntity
    {
        public required string Name { get; set; }

        public required string LastName { get; set; }

        public required string PhoneNumber { get; set; }

        public required string Email { get; set; }

        public required string Dni { get; set; }

        public required IberaPortales Portal { get; set; }

        public required string Password_hash { get; set; }

        //FK y Navegación completa del Rol
        public Guid IdRol { get; set; }

        // La Propiedad de Navegación (Navigation Property)
        // Esto te permite acceder a los datos del rol directamente en C# (ej: user.Rol.RolName)
        // "= null!" en vez de "required": EF la completa al hacer Include, no hace falta pasarla al crear.
        public virtual Role Rol { get; set; } = null!;

        //Navegacion inversa
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

        public virtual ICollection<RangerTask> AssignedTasks { get; set; } = new List<RangerTask>();
    }
}
