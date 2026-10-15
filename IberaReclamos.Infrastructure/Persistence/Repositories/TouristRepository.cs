using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Enum;
using IberaReclamos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IberaReclamos.Infrastructure.Persistence.Repositories
{
    public class TouristRepository : GenericRepository<Tourist>, ITouristRepository
    {
        public TouristRepository(IberaReclamosDbContext context) : base(context) { }

        public async Task<Tourist?> GetByClientCodeAsync(string clientCode)
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ClientCode == clientCode);
        }

        public async Task<IEnumerable<Tourist>> GetFilteredAsync(IberaPortales? portal, FeedbackKind? kind)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (portal.HasValue) query = query.Where(t => t.Portal == portal.Value);
            if (kind.HasValue) query = query.Where(t => t.Kind == kind.Value);

            return await query
                .OrderByDescending(t => t.CreateAt)
                .ToListAsync();
        }
    }
}
