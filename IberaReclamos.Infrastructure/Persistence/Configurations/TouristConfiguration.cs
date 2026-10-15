using IberaReclamos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IberaReclamos.Infrastructure.Persistence.Configurations
{
    public class TouristConfiguration : IEntityTypeConfiguration<Tourist>
    {
        public void Configure(EntityTypeBuilder<Tourist> builder)
        {
            builder.ToTable("Tourists", t =>
            {
                t.HasCheckConstraint("CK_Tourist_Evaluation", "\"Evaluation\" BETWEEN 1 AND 5");
            });

            builder.ConfigureAudit();

            builder.Property(t => t.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(t => t.Evaluation)
                .IsRequired();

            builder.Property(t => t.Kind)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(t => t.Email)
                .HasMaxLength(150);

            builder.Property(t => t.Portal)
                .IsRequired()
                .HasMaxLength(30)
                .HasConversion<string>();

            builder.Property(t => t.ClientCode)
                .HasMaxLength(20);

            builder.Property(t => t.PhotoKey)
                .HasMaxLength(300);

            builder.Property(t => t.PhotoUrl)
                .HasMaxLength(500);

            // Una valoración reenviada desde la cola offline del turista no se duplica
            builder.HasIndex(t => t.ClientCode).IsUnique();

            builder.HasIndex(t => t.Portal);
            builder.HasIndex(t => t.Kind);

            builder.HasQueryFilter(t => !t.IsDeleted);
        }
    }
}
