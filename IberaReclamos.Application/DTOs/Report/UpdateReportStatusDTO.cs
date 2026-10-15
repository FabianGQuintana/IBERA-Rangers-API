using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Application.DTOs.Report
{
    public class UpdateReportStatusDTO
    {
        public required TypeStatusReportAndTasks Status { get; set; }
    }
}
