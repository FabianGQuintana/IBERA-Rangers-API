using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Application.DTOs.Report
{
    // Filtros opcionales del listado (query string): ?status=Pending&categoryId=...&from=2026-10-01
    public class ReportFilterDTO
    {
        public TypeStatusReportAndTasks? Status { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? ReportedById { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }
    }
}
