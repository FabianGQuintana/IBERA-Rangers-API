using IberaReclamos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IberaReclamos.Infrastructure.Persistence.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Reports", t =>
            {
                // Coordenadas GPS válidas
                t.HasCheckConstraint("CK_Report_Latitude", "\"Latitude\" BETWEEN -90 AND 90");
                t.HasCheckConstraint("CK_Report_Longitude", "\"Longitude\" BETWEEN -180 AND 180");
            });

            builder.ConfigureAudit();

            builder.Property(r => r.Latitude).IsRequired();
            builder.Property(r => r.Longitude).IsRequired();

            builder.Property(r => r.Notes)
                .HasMaxLength(1000);

            builder.Property(r => r.CaptureDate)
                .IsRequired();

            builder.Property(r => r.StatusReport)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(r => r.PhotoKey)
                .HasMaxLength(300);

            builder.Property(r => r.PhotoUrl)
                .HasMaxLength(500);

            // Idempotencia de la sincronización offline: un mismo ClientId no se puede guardar dos veces.
            // (En PostgreSQL varios NULL no chocan entre sí, así que los reportes sin ClientId no molestan.)
            builder.HasIndex(r => r.ClientId).IsUnique();

            // Índices para los filtros más comunes del panel
            builder.HasIndex(r => r.StatusReport);
            builder.HasIndex(r => r.CaptureDate);

            // Relaciones (eliminación restrictiva: nunca se borra en cascada un reporte)
            builder.HasOne(r => r.Category)
                .WithMany(c => c.Reports)
                .HasForeignKey(r => r.ReportCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ReportedBy)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.ReportedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Borrado lógico: los reportes con IsDeleted = true no aparecen en las consultas
            builder.HasQueryFilter(r => !r.IsDeleted);
        }
    }
}
