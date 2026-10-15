using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Interfaces;
using IberaReclamos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace IberaReclamos.Infrastructure.Persistence
{
    // Envuelve la transacción de EF Core para que las capas de arriba no dependan de EF.
    internal sealed class DbTransactionWrapper : ITransaction
    {
        private readonly IDbContextTransaction _transaction;

        public DbTransactionWrapper(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync() => _transaction.CommitAsync();

        public Task RollbackAsync() => _transaction.RollbackAsync();

        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly IberaReclamosDbContext _context;

        // Campos privados (Lazy Initialization): cada repositorio se crea recién la primera vez que se usa
        private IUserRepository? _users;
        private IReportRepository? _reports;
        private IRangerTaskRepository? _tasks;
        private ITouristRepository? _tourists;
        private IGenericRepository<Role>? _roles;
        private IGenericRepository<ReportCategory>? _reportCategories;

        public UnitOfWork(IberaReclamosDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IReportRepository Reports => _reports ??= new ReportRepository(_context);
        public IRangerTaskRepository Tasks => _tasks ??= new RangerTaskRepository(_context);
        public ITouristRepository Tourists => _tourists ??= new TouristRepository(_context);
        public IGenericRepository<Role> Roles => _roles ??= new GenericRepository<Role>(_context);
        public IGenericRepository<ReportCategory> ReportCategories => _reportCategories ??= new GenericRepository<ReportCategory>(_context);

        // --- Gestión de Persistencia ---
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);

        // --- Gestión de Transacciones ---
        public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var efTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return new DbTransactionWrapper(efTransaction);
        }

        // El DbContext lo crea y lo libera el contenedor de DI (es Scoped), así que acá no lo desechamos.
        public void Dispose() => GC.SuppressFinalize(this);
    }
}
