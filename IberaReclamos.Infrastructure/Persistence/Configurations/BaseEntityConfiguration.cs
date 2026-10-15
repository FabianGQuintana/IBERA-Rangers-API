using IberaReclamos.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IberaReclamos.Infrastructure.Persistence.Configurations
{
    // Configuración común de las columnas de auditoría de BaseEntity.
    // Cada IEntityTypeConfiguration la llama con builder.ConfigureAudit() para no repetir lo mismo 6 veces.
    public static class BaseEntityConfiguration
    {
        public static void ConfigureAudit<T>(this EntityTypeBuilder<T> builder) where T : BaseEntity
        {
            builder.HasKey(e => e.ID);

            // El Guid lo genera C# (Guid.NewGuid() en BaseEntity), no la base de datos.
            builder.Property(e => e.ID)
                .ValueGeneratedNever();

            builder.Property(e => e.CreateBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.LastModifiedBy)
                .HasMaxLength(100);

            builder.Property(e => e.CreateAt)
                .IsRequired();

            builder.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.IsDeleted)
                .IsRequired();
        }
    }
}
