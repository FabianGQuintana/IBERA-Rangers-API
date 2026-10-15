using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Domain.Interfaces
{
    public interface IReportRepository : IGenericRepository<Report>
    {
        // Incluye categoría y ranger que reportó
        Task<Report?> GetByIdWithDetailsAsync(Guid id);

        Task<IEnumerable<Report>> GetFilteredAsync(
            TypeStatusReportAndTasks? status,
            Guid? categoryId,
            Guid? reportedById,
            DateTime? from,
            DateTime? to);

        // Para la sincronización offline: de estos ClientId, ¿cuáles ya están guardados? (ClientId -> Id del servidor)
        Task<Dictionary<Guid, Guid>> GetIdsByClientIdsAsync(IEnumerable<Guid> clientIds);
    }
}
