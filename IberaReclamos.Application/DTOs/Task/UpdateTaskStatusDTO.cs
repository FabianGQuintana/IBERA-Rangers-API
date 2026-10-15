using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Application.DTOs.Task
{
    public class UpdateTaskStatusDTO
    {
        public required TypeStatusReportAndTasks Status { get; set; }
    }
}
