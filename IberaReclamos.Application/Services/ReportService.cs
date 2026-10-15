using IberaReclamos.Application.Common;
using IberaReclamos.Application.DTOs.Report;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Mappers;
using IberaReclamos.Application.Models;
using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Exceptions;
using IberaReclamos.Domain.Interfaces;

namespace IberaReclamos.Application.Services
{
    public class ReportService : IReportService
    {
        private const string PhotoFolder = "reports";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storage;

        public ReportService(IUnitOfWork unitOfWork, IStorageService storage)
        {
            _unitOfWork = unitOfWork;
            _storage = storage;
        }

        public async Task<IEnumerable<ReportResponseDTO>> GetFilteredAsync(ReportFilterDTO filter)
        {
            var reports = await _unitOfWork.Reports.GetFilteredAsync(
                filter.Status,
                filter.CategoryId,
                filter.ReportedById,
                DateHelper.ToUtc(filter.From),
                DateHelper.ToUtc(filter.To));

            return reports.Select(r => r.ToDto());
        }

        public async Task<ReportResponseDTO> GetByIdAsync(Guid id)
        {
            var report = await _unitOfWork.Reports.GetByIdWithDetailsAsync(id)
                ?? throw new NotFoundException("El reporte no existe.");

            return report.ToDto();
        }

        public async Task<ReportResponseDTO> CreateAsync(CreateReportDTO dto, Guid reportedById, FileUpload? photo, CancellationToken cancellationToken = default)
        {
            // Si el mismo reporte ya llegó (reintento con el mismo ClientId), devolvemos el existente en vez de duplicarlo
            if (dto.ClientId.HasValue)
            {
                var existing = await _unitOfWork.Reports.GetIdsByClientIdsAsync(new[] { dto.ClientId.Value });
                if (existing.TryGetValue(dto.ClientId.Value, out var existingId))
                    return await GetByIdAsync(existingId);
            }

            await EnsureCategoriesExistAsync(new[] { dto.ReportCategoryId });

            var report = dto.ToEntity(reportedById, DateTime.UtcNow);

            // La foto se sube ANTES de guardar: si R2 falla no queda un reporte "a medias" esperando una foto
            string? uploadedKey = null;
            if (photo is not null)
            {
                uploadedKey = await UploadPhotoToStorageAsync(report, photo, cancellationToken);
            }

            try
            {
                await _unitOfWork.Reports.AddAsync(report);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Si la DB falla, borramos la foto para no dejar archivos huérfanos en R2
                if (uploadedKey is not null) await _storage.DeleteAsync(uploadedKey, CancellationToken.None);
                throw;
            }

            return await GetByIdAsync(report.ID);
        }

        // =====================================================================
        // BULK INSERT: sincronización offline de los rangers
        // =====================================================================
        // 1. Busca qué ClientId ya están guardados (reintentos) y los saltea: la operación es idempotente.
        // 2. Valida que todas las categorías existan con UNA sola consulta.
        // 3. Inserta todos los nuevos con AddRange + un único SaveChanges dentro de una transacción:
        //    o se guardan todos, o ninguno.
        public async Task<SyncReportsResultDTO> SyncAsync(SyncReportsDTO dto, Guid reportedById, CancellationToken cancellationToken = default)
        {
            var result = new SyncReportsResultDTO { Received = dto.Reports.Count };

            var clientIds = dto.Reports.Select(r => r.ClientId!.Value).ToList();
            var alreadySynced = await _unitOfWork.Reports.GetIdsByClientIdsAsync(clientIds);

            var pending = dto.Reports
                .Where(r => !alreadySynced.ContainsKey(r.ClientId!.Value))
                .ToList();

            await EnsureCategoriesExistAsync(pending.Select(r => r.ReportCategoryId));

            var syncDate = DateTime.UtcNow;
            var newReports = pending.Select(r => r.ToEntity(reportedById, syncDate)).ToList();

            if (newReports.Count > 0)
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _unitOfWork.Reports.AddRangeAsync(newReports);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            // Respuesta en el mismo orden en que llegaron, para que la app marque cada uno como sincronizado
            var createdByClientId = newReports.ToDictionary(r => r.ClientId!.Value, r => r.ID);
            foreach (var clientId in clientIds)
            {
                var created = createdByClientId.TryGetValue(clientId, out var reportId);
                if (!created) reportId = alreadySynced[clientId];

                result.Items.Add(new SyncedReportDTO { ClientId = clientId, ReportId = reportId, Created = created });
            }

            result.Created = newReports.Count;
            result.AlreadySynced = result.Received - result.Created;
            return result;
        }

        public async Task<ReportResponseDTO> UploadPhotoAsync(Guid reportId, FileUpload photo, Guid userId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(reportId)
                ?? throw new NotFoundException("El reporte no existe.");

            if (!isAdmin && report.ReportedById != userId)
                throw new ForbiddenException("Solo podés subir fotos a tus propios reportes.");

            var previousKey = report.PhotoKey;
            var uploadedKey = await UploadPhotoToStorageAsync(report, photo, cancellationToken);
            report.LastModifiedBy = userId.ToString();

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await _storage.DeleteAsync(uploadedKey, CancellationToken.None);
                throw;
            }

            // Recién cuando la DB apunta a la foto nueva borramos la anterior
            if (previousKey is not null) await _storage.DeleteAsync(previousKey, CancellationToken.None);

            return await GetByIdAsync(reportId);
        }

        public async Task<ReportResponseDTO> UpdateStatusAsync(Guid id, UpdateReportStatusDTO dto, string modifiedBy)
        {
            var report = await _unitOfWork.Reports.GetByIdAsync(id)
                ?? throw new NotFoundException("El reporte no existe.");

            report.StatusReport = dto.Status;
            report.LastModifiedBy = modifiedBy;

            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        // ---------------------------------------------------------------------

        private async Task EnsureCategoriesExistAsync(IEnumerable<Guid> categoryIds)
        {
            var ids = categoryIds.Distinct().ToList();
            if (ids.Count == 0) return;

            var found = await _unitOfWork.ReportCategories.WhereAsync(c => ids.Contains(c.ID) && !c.IsDeleted);
            var missing = ids.Except(found.Select(c => c.ID)).ToList();

            if (missing.Count > 0)
                throw new BadRequestException($"La categoría {missing[0]} no existe.");
        }

        private async Task<string> UploadPhotoToStorageAsync(Report report, FileUpload photo, CancellationToken cancellationToken)
        {
            var (extension, contentType) = await PhotoRules.EnsureValidAsync(photo, cancellationToken);
            var key = PhotoRules.BuildObjectKey(PhotoFolder, extension);

            var uploaded = await _storage.UploadAsync(photo.Content, key, contentType, cancellationToken);

            report.PhotoKey = uploaded.ObjectKey;
            report.PhotoUrl = uploaded.PublicUrl;
            return uploaded.ObjectKey;
        }
    }
}
