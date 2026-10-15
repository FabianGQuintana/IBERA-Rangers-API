using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IberaReclamos.Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        // Ids fijos para que los roles existan desde la primera migración
        // y el código pueda referenciarlos sin consultar la DB.
        public static readonly Guid AdminRoleId = Guid.Parse("8d1f2c3a-0001-4a6e-9b1a-000000000001");
        public static readonly Guid RangerRoleId = Guid.Parse("8d1f2c3a-0001-4a6e-9b1a-000000000002");
        public static readonly Guid HelperRoleId = Guid.Parse("8d1f2c3a-0001-4a6e-9b1a-000000000003");

        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.ConfigureAudit();

            builder.Property(r => r.RolName)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>(); // Convierte el Enum a String en la DB

            builder.Property(r => r.Description)
                .HasMaxLength(250);

            // No puede haber dos roles con el mismo nombre
            builder.HasIndex(r => r.RolName).IsUnique();

            // Datos semilla (HasData necesita valores fijos: nada de Guid.NewGuid() ni DateTime.UtcNow)
            var seedDate = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc);
            builder.HasData(
                new Role { ID = AdminRoleId, RolName = tipoRol.Admin, Description = "Administrador del sistema", CreateAt = seedDate, CreateBy = "Seed" },
                new Role { ID = RangerRoleId, RolName = tipoRol.Ranger, Description = "Guardaparque que reporta incidentes en el terreno", CreateAt = seedDate, CreateBy = "Seed" },
                new Role { ID = HelperRoleId, RolName = tipoRol.Helper, Description = "Personal de apoyo", CreateAt = seedDate, CreateBy = "Seed" }
            );
        }
    }
}
