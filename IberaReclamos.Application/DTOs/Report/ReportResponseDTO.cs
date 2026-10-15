namespace IberaReclamos.Application.DTOs.Report
{
    public class ReportResponseDTO
    {
        public Guid Id { get; set; }

        public Guid? ClientId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string? Notes { get; set; }

        public DateTime CaptureDate { get; set; }

        public DateTime? SyncDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public Guid ReportCategoryId { get; set; }

        public string Category { get; set; } = string.Empty;

        public string SeverityLevel { get; set; } = string.Empty;

        public Guid ReportedById { get; set; }

        public string ReportedBy { get; set; } = string.Empty;
    }
}
