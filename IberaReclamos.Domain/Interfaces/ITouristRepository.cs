using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Domain.Interfaces
{
    public interface ITouristRepository : IGenericRepository<Tourist>
    {
        // Para no duplicar una valoración reenviada desde la cola offline del turista
        Task<Tourist?> GetByClientCodeAsync(string clientCode);

        Task<IEnumerable<Tourist>> GetFilteredAsync(IberaPortales? portal, FeedbackKind? kind);
    }
}
