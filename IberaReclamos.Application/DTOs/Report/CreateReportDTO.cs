namespace IberaReclamos.Application.DTOs.Report
{
    // Un reporte tomado por el ranger en el terreno. La foto viaja aparte (multipart o POST /reports/{id}/photo).
    public class CreateReportDTO
    {
        // Id que genera el dispositivo offline. Obligatorio al sincronizar para no duplicar reportes.
        public Guid? ClientId { get; set; }

        public required double Latitude { get; set; }

        public required double Longitude { get; set; }

        public string? Notes { get; set; }

        // Cuándo se tomó en el terreno (ISO 8601, idealmente con zona horaria).
        public required DateTime CaptureDate { get; set; }

        public required Guid ReportCategoryId { get; set; }
    }
}
