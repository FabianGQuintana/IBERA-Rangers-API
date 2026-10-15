using System.Linq.Expressions;
using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IberaReclamos.Infrastructure.Persistence.Repositories
{
    //Implementa IGenericRepository<T> para cualquier entidad que herede de BaseEntity.
    //Los repositorios específicos (UserRepository, ReportRepository...) heredan de esta clase.
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly IberaReclamosDbContext _context;

        protected readonly DbSet<T> _dbSet;

        public GenericRepository(IberaReclamosDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IQueryable<T> GetQueryable() => _dbSet;

        public virtual async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.AsNoTracking().ToListAsync();

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        // Bulk insert: EF agrupa todos los INSERT y los manda juntos en el SaveChanges (batching)
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Actualizar: EF Core rastrea la entidad y marca lo que cambió
        public Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            // El guardado real lo hace el UnitOfWork.SaveChangesAsync()
            return Task.FromResult(entity);
        }

        // Soft Delete Toggle: si está borrada la restaura; si no, la borra (lógicamente).
        public async Task<T?> SoftDeleteToggleAsync(Guid id)
        {
            // IgnoreQueryFilters: necesitamos encontrarla aunque ya esté borrada (para poder restaurarla)
            var entity = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.ID == id);

            if (entity == null) return null;

            if (entity.IsDeleted)
            {
                // Restaurar
                entity.IsDeleted = false;
                entity.DeleteAt = null;
                entity.Status = "Active";
            }
            else
            {
                // Borrado Lógico
                entity.IsDeleted = true;
                entity.DeleteAt = DateTime.UtcNow;
                entity.Status = "Inactive";
            }

            return entity;
        }

        public Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }
    }
}
