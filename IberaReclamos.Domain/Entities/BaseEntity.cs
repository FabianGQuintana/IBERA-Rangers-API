using System;
using System.Collections.Generic;
using System.Text;

namespace IberaReclamos.Domain.Entities
{
    public abstract class BaseEntity //Clase para atributos de auditoria de las entidades puras.
    {
        public Guid ID { get; set; } = Guid.NewGuid();

        public string CreateBy { get; set; } = "Admin";

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastModifiedAt { get; set; }

        public DateTime? DeleteAt { get; set; }

        public string? LastModifiedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
         
        public string Status { get; set; } = "Active";


    }
}
