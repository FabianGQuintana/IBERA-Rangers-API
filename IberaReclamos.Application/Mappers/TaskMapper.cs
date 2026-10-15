using IberaReclamos.Application.Common;
using IberaReclamos.Application.DTOs.Task;
using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Application.Mappers
{
    public static class TaskMapper
    {
        public static TaskResponseDTO ToDto(this RangerTask entity)
        {
            return new TaskResponseDTO
            {
                Id = entity.ID,
                Name = entity.Name,
                Description = entity.Description,
                Status = entity.StatusTask.ToString(),
                DueDate = entity.DueDate,
                AssignedById = entity.AssignedById,
                AssignedBy = entity.AssignedBy is null ? string.Empty : $"{entity.AssignedBy.Name} {entity.AssignedBy.LastName}",
                AssignedToId = entity.AssignedToId,
                AssignedTo = entity.AssignedTo is null ? string.Empty : $"{entity.AssignedTo.Name} {entity.AssignedTo.LastName}",
                ReportId = entity.ReportId,
                CreateAt = entity.CreateAt
            };
        }

        public static RangerTask ToEntity(this CreateTaskDTO dto, Guid assignedById)
        {
            return new RangerTask
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                StatusTask = TypeStatusReportAndTasks.Pending,
                DueDate = DateHelper.ToUtc(dto.DueDate),
                AssignedById = assignedById,
                AssignedToId = dto.AssignedToId,
                ReportId = dto.ReportId,
                CreateBy = assignedById.ToString()
            };
        }
    }
}
