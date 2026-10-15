using IberaReclamos.Application.DTOs.Task;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Mappers;
using IberaReclamos.Domain.Enum;
using IberaReclamos.Domain.Exceptions;
using IberaReclamos.Domain.Interfaces;

namespace IberaReclamos.Application.Services
{
    public class RangerTaskService : IRangerTaskService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RangerTaskService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<IEnumerable<TaskResponseDTO>> GetAllAsync()
        {
            var tasks = await _unitOfWork.Tasks.GetAllWithDetailsAsync();
            return tasks.Select(t => t.ToDto());
        }

        public async Task<IEnumerable<TaskResponseDTO>> GetByAssigneeAsync(Guid rangerId)
        {
            var tasks = await _unitOfWork.Tasks.GetByAssigneeAsync(rangerId);
            return tasks.Select(t => t.ToDto());
        }

        public async Task<TaskResponseDTO> CreateAsync(CreateTaskDTO dto, Guid assignedById)
        {
            var assignee = await _unitOfWork.Users.GetByIdWithRoleAsync(dto.AssignedToId)
                ?? throw new BadRequestException("El usuario al que querés asignar la tarea no existe.");

            if (assignee.IsDeleted)
                throw new BadRequestException("No se pueden asignar tareas a un usuario desactivado.");

            if (assignee.Rol.RolName == tipoRol.Admin)
                throw new BadRequestException("Las tareas se asignan a rangers o personal de apoyo, no a administradores.");

            if (dto.ReportId.HasValue && !await _unitOfWork.Reports.AnyAsync(r => r.ID == dto.ReportId.Value))
                throw new BadRequestException("El reporte asociado no existe.");

            var task = dto.ToEntity(assignedById);

            await _unitOfWork.Tasks.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Tasks.GetByIdWithDetailsAsync(task.ID);
            return created!.ToDto();
        }

        public async Task<TaskResponseDTO> UpdateStatusAsync(Guid id, UpdateTaskStatusDTO dto, Guid userId, bool isAdmin)
        {
            var task = await _unitOfWork.Tasks.GetByIdWithDetailsAsync(id)
                ?? throw new NotFoundException("La tarea no existe.");

            if (!isAdmin && task.AssignedToId != userId)
                throw new ForbiddenException("Solo podés actualizar las tareas que te asignaron.");

            task.StatusTask = dto.Status;
            task.LastModifiedBy = userId.ToString();

            await _unitOfWork.SaveChangesAsync();

            return task.ToDto();
        }
    }
}
