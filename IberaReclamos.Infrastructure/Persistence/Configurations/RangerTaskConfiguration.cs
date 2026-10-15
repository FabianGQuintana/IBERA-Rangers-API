using IberaReclamos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IberaReclamos.Infrastructure.Persistence.Configurations
{
    public class RangerTaskConfiguration : IEntityTypeConfiguration<RangerTask>
    {
        public void Configure(EntityTypeBuilder<RangerTask> builder)
        {
            builder.ToTable("Tasks");

            builder.ConfigureAudit();

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(t => t.Description)
                .HasMaxLength(1000);

            builder.Property(t => t.StatusTask)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.HasIndex(t => t.StatusTask);

            // Dos relaciones hacia User: hay que decirle a EF cuál es cuál
            builder.HasOne(t => t.AssignedBy)
                .WithMany()
                .HasForeignKey(t => t.AssignedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Report)
                .WithMany(r => r.Tasks)
                .HasForeignKey(t => t.ReportId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(t => !t.IsDeleted);
        }
    }
}
