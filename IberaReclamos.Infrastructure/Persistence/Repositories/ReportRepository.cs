using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Enum;
using IberaReclamos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IberaReclamos.Infrastructure.Persistence.Repositories
{
    public class ReportRepository : GenericRepository<Report>, IReportRepository
    {
        public ReportRepository(IberaReclamosDbContext context) : base(context) { }

        public async Task<Report?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(r => r.Category)
                .Include(r => r.ReportedBy)
                .FirstOrDefaultAsync(r => r.ID == id);
        }

        public async Task<IEnumerable<Report>> GetFilteredAsync(
            TypeStatusReportAndTasks? status,
            Guid? categoryId,
            Guid? reportedById,
            DateTime? from,
            DateTime? to)
        {
            // Armamos la consulta de a poco: cada filtro solo se agrega si vino un valor.
            // Nada se ejecuta en la DB hasta el ToListAsync().
            var query = _dbSet
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.ReportedBy)
                .AsQueryable();

            if (status.HasValue) query = query.Where(r => r.StatusReport == status.Value);
            if (categoryId.HasValue) query = query.Where(r => r.ReportCategoryId == categoryId.Value);
            if (reportedById.HasValue) query = query.Where(r => r.ReportedById == reportedById.Value);
            if (from.HasValue) query = query.Where(r => r.CaptureDate >= from.Value);
            if (to.HasValue) query = query.Where(r => r.CaptureDate <= to.Value);

            return await query
                .OrderByDescending(r => r.CaptureDate)
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, Guid>> GetIdsByClientIdsAsync(IEnumerable<Guid> clientIds)
        {
            var ids = clientIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<Guid, Guid>();

            // IgnoreQueryFilters: un reporte borrado lógicamente también ocupa su ClientId (índice único)
            return await _dbSet
                .IgnoreQueryFilters()
                .Where(r => r.ClientId.HasValue && ids.Contains(r.ClientId.Value))
                .ToDictionaryAsync(r => r.ClientId!.Value, r => r.ID);
        }
    }
}
