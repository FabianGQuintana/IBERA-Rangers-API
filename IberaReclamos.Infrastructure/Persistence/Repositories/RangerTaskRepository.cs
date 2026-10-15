using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IberaReclamos.Infrastructure.Persistence.Repositories
{
    public class RangerTaskRepository : GenericRepository<RangerTask>, IRangerTaskRepository
    {
        public RangerTaskRepository(IberaReclamosDbContext context) : base(context) { }

        public async Task<RangerTask?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(t => t.AssignedBy)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.ID == id);
        }

        public async Task<IEnumerable<RangerTask>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(t => t.AssignedBy)
                .Include(t => t.AssignedTo)
                .OrderByDescending(t => t.CreateAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RangerTask>> GetByAssigneeAsync(Guid assignedToId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(t => t.AssignedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.AssignedToId == assignedToId)
                .OrderByDescending(t => t.CreateAt)
                .ToListAsync();
        }
    }
}
