using IberaReclamos.Domain.Entities;

namespace IberaReclamos.Domain.Interfaces
{
    public interface IRangerTaskRepository : IGenericRepository<RangerTask>
    {
        Task<RangerTask?> GetByIdWithDetailsAsync(Guid id);

        Task<IEnumerable<RangerTask>> GetAllWithDetailsAsync();

        // Las tareas asignadas a un ranger (su "bandeja de entrada")
        Task<IEnumerable<RangerTask>> GetByAssigneeAsync(Guid assignedToId);
    }
}
