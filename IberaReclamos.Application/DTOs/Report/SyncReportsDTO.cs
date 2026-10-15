namespace IberaReclamos.Application.DTOs.Report
{
    // Bulk insert: el ranger recupera señal y manda todos los reportes que guardó offline de una sola vez.
    public class SyncReportsDTO
    {
        public List<CreateReportDTO> Reports { get; set; } = new();
    }
}
