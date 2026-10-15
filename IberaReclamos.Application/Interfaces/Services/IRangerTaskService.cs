using IberaReclamos.Application.DTOs.Task;

namespace IberaReclamos.Application.Interfaces.Services
{
    public interface IRangerTaskService
    {
        Task<IEnumerable<TaskResponseDTO>> GetAllAsync();

        Task<IEnumerable<TaskResponseDTO>> GetByAssigneeAsync(Guid rangerId);

        Task<TaskResponseDTO> CreateAsync(CreateTaskDTO dto, Guid assignedById);

        // Un ranger solo puede cambiar el estado de SUS tareas; un admin, de cualquiera.
        Task<TaskResponseDTO> UpdateStatusAsync(Guid id, UpdateTaskStatusDTO dto, Guid userId, bool isAdmin);
    }
}
