using IberaReclamos.Application.Common;
using IberaReclamos.Application.DTOs.Report;
using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Application.Mappers
{
    public static class ReportMapper
    {
        public static ReportResponseDTO ToDto(this Report entity)
        {
            return new ReportResponseDTO
            {
                Id = entity.ID,
                ClientId = entity.ClientId,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Notes = entity.Notes,
                CaptureDate = entity.CaptureDate,
                SyncDate = entity.SyncDate,
                Status = entity.StatusReport.ToString(),
                PhotoUrl = entity.PhotoUrl,
                ReportCategoryId = entity.ReportCategoryId,
                // Category y ReportedBy vienen cargados solo si el repositorio hizo Include
                Category = entity.Category?.Name ?? string.Empty,
                SeverityLevel = entity.Category?.SeverityLevel.ToString() ?? string.Empty,
                ReportedById = entity.ReportedById,
                ReportedBy = entity.ReportedBy is null ? string.Empty : $"{entity.ReportedBy.Name} {entity.ReportedBy.LastName}"
            };
        }

        // reportedById sale del token del ranger logueado, nunca del body (así nadie reporta en nombre de otro).
        public static Report ToEntity(this CreateReportDTO dto, Guid reportedById, DateTime syncDate)
        {
            return new Report
            {
                ClientId = dto.ClientId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                CaptureDate = DateHelper.ToUtc(dto.CaptureDate),
                SyncDate = syncDate,
                StatusReport = TypeStatusReportAndTasks.Pending,
                ReportCategoryId = dto.ReportCategoryId,
                ReportedById = reportedById,
                CreateBy = reportedById.ToString()
            };
        }
    }
}
