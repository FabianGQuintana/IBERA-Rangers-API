using IberaReclamos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IberaReclamos.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // 1. Tabla y Check Constraint para el DNI (solo números, 7 u 8 dígitos)
            builder.ToTable("Users", t =>
            {
                // En PostgreSQL "~" es comparación con expresión regular
                t.HasCheckConstraint("CK_User_Dni_Format", "\"Dni\" ~ '^[0-9]{7,8}$'");
            });

            // 2. PK y auditoría heredadas de BaseEntity
            builder.ConfigureAudit();

            // 3. Propiedades (MaxLengths para optimizar la DB)
            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(25);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(u => u.Dni)
                .IsRequired()
                .HasMaxLength(8); // Máximo 8 caracteres

            builder.Property(u => u.Password_hash)
                .IsRequired()
                .HasMaxLength(255); // Un hash de BCrypt mide 60 caracteres; dejamos margen

            // 4. Configurar el Enum para que se guarde como Texto en la DB
            builder.Property(u => u.Portal)
                .IsRequired()
                .HasMaxLength(30)
                .HasConversion<string>();

            // 5. Índices únicos: no puede haber dos usuarios con el mismo email o DNI
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.Dni).IsUnique();

            // =========================================================
            // 6. RELACIÓN CON EL ROL
            // =========================================================
            builder.HasOne(u => u.Rol)                  // Un Usuario TIENE UN Rol
                .WithMany(r => r.Users)                 // Un Rol tiene MUCHOS Usuarios (usa la colección Role.Users)
                .HasForeignKey(u => u.IdRol)            // La FK es IdRol
                .OnDelete(DeleteBehavior.Restrict);     // Previene borrar un rol si hay usuarios usándolo
        }
    }
}
