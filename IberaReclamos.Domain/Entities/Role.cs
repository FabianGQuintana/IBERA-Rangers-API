using System;
using System.Collections.Generic;
using System.Text;
using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Domain.Entities
{
    public class Role : BaseEntity
    {
        public required tipoRol RolName { get; set; }

        public string? Description { get; set; }

        // Propiedad de navegación inversa: 
        // Le dice a EF Core "Un rol tiene una lista de usuarios"
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
