using IberaReclamos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IberaReclamos.Infrastructure
{
    public class IberaReclamosDbContext : DbContext
    {
        public IberaReclamosDbContext(
            DbContextOptions<IberaReclamosDbContext> options) : base(options)
        {

        }

        #region DBsets (tablas)
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportCategory> ReportCategories { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RangerTask> Tasks { get; set; }
        public DbSet<Tourist> Tourists { get; set; }
        public DbSet<User> Users { get; set; }
        #endregion


        //Metodo que se dispara cuando el sistema arma el modelo: busca en este proyecto
        //todas las clases IEntityTypeConfiguration<T> (carpeta Persistence/Configurations) y las aplica.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }


        // ==========================================
        // AUDITORÍA AUTOMÁTICA
        // ==========================================
        // Antes de guardar, completamos las fechas de auditoría de toda entidad que herede de BaseEntity,
        // así ningún servicio se olvida de hacerlo.
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreateAt = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = now;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
