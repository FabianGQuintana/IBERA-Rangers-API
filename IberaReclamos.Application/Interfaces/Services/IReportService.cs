using IberaReclamos.Application.DTOs.Report;
using IberaReclamos.Application.Models;

namespace IberaReclamos.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<IEnumerable<ReportResponseDTO>> GetFilteredAsync(ReportFilterDTO filter);

        Task<ReportResponseDTO> GetByIdAsync(Guid id);

        // Un reporte suelto (con conexión), con foto opcional
        Task<ReportResponseDTO> CreateAsync(CreateReportDTO dto, Guid reportedById, FileUpload? photo, CancellationToken cancellationToken = default);

        // Bulk insert: sincroniza de una vez todos los reportes que el ranger guardó offline
        Task<SyncReportsResultDTO> SyncAsync(SyncReportsDTO dto, Guid reportedById, CancellationToken cancellationToken = default);

        // Sube (o reemplaza) la foto de un reporte ya sincronizado
        Task<ReportResponseDTO> UploadPhotoAsync(Guid reportId, FileUpload photo, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);

        Task<ReportResponseDTO> UpdateStatusAsync(Guid id, UpdateReportStatusDTO dto, string modifiedBy);
    }
}
