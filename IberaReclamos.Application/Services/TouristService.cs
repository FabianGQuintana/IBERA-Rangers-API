using IberaReclamos.Application.Common;
using IberaReclamos.Application.DTOs.Tourist;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Mappers;
using IberaReclamos.Application.Models;
using IberaReclamos.Domain.Enum;
using IberaReclamos.Domain.Exceptions;
using IberaReclamos.Domain.Interfaces;

namespace IberaReclamos.Application.Services
{
    public class TouristService : ITouristService
    {
        private const string PhotoFolder = "tourists";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storage;

        public TouristService(IUnitOfWork unitOfWork, IStorageService storage)
        {
            _unitOfWork = unitOfWork;
            _storage = storage;
        }

        public async Task<FeedbackCreatedDTO> SubmitFeedbackAsync(CreateTouristFeedbackDTO dto, FileUpload? photo, CancellationToken cancellationToken = default)
        {
            // Honeypot: si un bot completó el campo oculto, respondemos "OK" pero no guardamos nada
            // (así el bot no se entera de que lo detectamos).
            if (!string.IsNullOrWhiteSpace(dto.Website))
                return new FeedbackCreatedDTO { Code = dto.ClientCode ?? NewCode() };

            // Reenvío desde la cola offline del turista: si ya la tenemos, devolvemos el mismo código sin duplicar
            if (!string.IsNullOrWhiteSpace(dto.ClientCode))
            {
                var existing = await _unitOfWork.Tourists.GetByClientCodeAsync(dto.ClientCode.Trim());
                if (existing is not null)
                    return new FeedbackCreatedDTO { Code = existing.ClientCode! };
            }

            var feedback = dto.ToEntity();
            feedback.ClientCode ??= NewCode();

            string? uploadedKey = null;
            if (photo is not null)
            {
                var (extension, contentType) = await PhotoRules.EnsureValidAsync(photo, cancellationToken);
                var uploaded = await _storage.UploadAsync(photo.Content, PhotoRules.BuildObjectKey(PhotoFolder, extension), contentType, cancellationToken);

                uploadedKey = uploaded.ObjectKey;
                feedback.PhotoKey = uploaded.ObjectKey;
                feedback.PhotoUrl = uploaded.PublicUrl;
            }

            try
            {
                await _unitOfWork.Tourists.AddAsync(feedback);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                if (uploadedKey is not null) await _storage.DeleteAsync(uploadedKey, CancellationToken.None);
                throw;
            }

            return new FeedbackCreatedDTO { Code = feedback.ClientCode };
        }

        public async Task<IEnumerable<TouristResponseDTO>> GetFilteredAsync(string? portal, string? kind)
        {
            IberaPortales? portalFilter = null;
            if (!string.IsNullOrWhiteSpace(portal))
            {
                if (!PortalSlugs.TryParse(portal, out var parsed))
                    throw new BadRequestException("El portal no es válido.");
                portalFilter = parsed;
            }

            FeedbackKind? kindFilter = null;
            if (!string.IsNullOrWhiteSpace(kind))
            {
                if (!FeedbackKinds.TryParse(kind, out var parsed))
                    throw new BadRequestException("El tipo de valoración no es válido.");
                kindFilter = parsed;
            }

            var items = await _unitOfWork.Tourists.GetFilteredAsync(portalFilter, kindFilter);
            return items.Select(t => t.ToDto());
        }

        public async Task<TouristResponseDTO> GetByIdAsync(Guid id)
        {
            var feedback = await _unitOfWork.Tourists.GetByIdAsync(id)
                ?? throw new NotFoundException("La valoración no existe.");

            return feedback.ToDto();
        }

        // Mismo formato que genera el frontend (IB-V1234), con más dígitos para que sea difícil que se repita
        private static string NewCode() => $"IB-V{Random.Shared.Next(100000, 999999)}";
    }
}
