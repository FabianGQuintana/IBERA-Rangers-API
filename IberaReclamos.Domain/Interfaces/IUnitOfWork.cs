using IberaReclamos.Domain.Entities;

namespace IberaReclamos.Domain.Interfaces
{
    // Agrupa todos los repositorios sobre el MISMO DbContext: los cambios de varios repositorios
    // se guardan juntos con un único SaveChangesAsync().
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }

        IReportRepository Reports { get; }

        IRangerTaskRepository Tasks { get; }

        ITouristRepository Tourists { get; }

        // Estas entidades no necesitan consultas especiales: alcanza con el genérico
        IGenericRepository<Role> Roles { get; }

        IGenericRepository<ReportCategory> ReportCategories { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); // devuelve el número de filas afectadas

        Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
