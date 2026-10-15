namespace IberaReclamos.Application.DTOs.Report
{
    public class SyncReportsResultDTO
    {
        public int Received { get; set; }

        // Reportes nuevos guardados en esta sincronización.
        public int Created { get; set; }

        // Reportes que ya existían (mismo ClientId): se ignoran sin error.
        public int AlreadySynced { get; set; }

        // Relación ClientId (dispositivo) -> ReportId (servidor), para que la app suba las fotos
        // y marque cada reporte local como sincronizado.
        public List<SyncedReportDTO> Items { get; set; } = new();
    }

    public class SyncedReportDTO
    {
        public Guid ClientId { get; set; }

        public Guid ReportId { get; set; }

        public bool Created { get; set; }
    }
}
