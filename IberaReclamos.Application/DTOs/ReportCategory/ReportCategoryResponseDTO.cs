namespace IberaReclamos.Application.DTOs.ReportCategory
{
    public class ReportCategoryResponseDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string SeverityLevel { get; set; } = string.Empty;
    }
}
