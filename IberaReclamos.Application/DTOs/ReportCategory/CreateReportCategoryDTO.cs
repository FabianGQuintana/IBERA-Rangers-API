using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Application.DTOs.ReportCategory
{
    public class CreateReportCategoryDTO
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        public required SeverityLevelReport SeverityLevel { get; set; }
    }
}
