using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IberaReclamos.Infrastructure.Persistence.Configurations
{
    public class ReportCategoryConfiguration : IEntityTypeConfiguration<ReportCategory>
    {
        public void Configure(EntityTypeBuilder<ReportCategory> builder)
        {
            builder.ToTable("ReportCategories");

            builder.ConfigureAudit();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.SeverityLevel)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.HasIndex(c => c.Name).IsUnique();

            // Categorías iniciales para que los rangers puedan reportar desde el primer día
            var seedDate = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc);
            builder.HasData(
                Category("8d1f2c3a-0002-4a6e-9b1a-000000000001", "Lixiviados", "Líquidos contaminantes que escurren de residuos.", SeverityLevelReport.High),
                Category("8d1f2c3a-0002-4a6e-9b1a-000000000002", "Plásticos y residuos", "Basura o plásticos abandonados en el área protegida.", SeverityLevelReport.Moderate),
                Category("8d1f2c3a-0002-4a6e-9b1a-000000000003", "Incendio", "Foco de incendio o quema activa.", SeverityLevelReport.Emergency),
                Category("8d1f2c3a-0002-4a6e-9b1a-000000000004", "Caza o pesca furtiva", "Indicios o presencia de caza/pesca ilegal.", SeverityLevelReport.Critical),
                Category("8d1f2c3a-0002-4a6e-9b1a-000000000005", "Fauna herida o muerta", "Animal herido, enfermo o muerto.", SeverityLevelReport.High),
                Category("8d1f2c3a-0002-4a6e-9b1a-000000000006", "Infraestructura dañada", "Senderos, carteles o pasarelas en mal estado.", SeverityLevelReport.Low),
                Category("8d1f2c3a-0002-4a6e-9b1a-000000000007", "Otro", "Incidente que no encaja en las demás categorías.", SeverityLevelReport.Low)
            );

            ReportCategory Category(string id, string name, string description, SeverityLevelReport severity) => new()
            {
                ID = Guid.Parse(id),
                Name = name,
                Description = description,
                SeverityLevel = severity,
                CreateAt = seedDate,
                CreateBy = "Seed"
            };
        }
    }
}
