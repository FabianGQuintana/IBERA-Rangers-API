using System.Globalization;
using IberaReclamos.Application.Common;
using IberaReclamos.Application.DTOs.Tourist;
using IberaReclamos.Domain.Entities;

namespace IberaReclamos.Application.Mappers
{
    public static class TouristMapper
    {
        public static TouristResponseDTO ToDto(this Tourist entity)
        {
            return new TouristResponseDTO
            {
                Id = entity.ID,
                Portal = PortalSlugs.ToSlug(entity.Portal),
                Kind = FeedbackKinds.ToSlug(entity.Kind),
                Rating = entity.Evaluation,
                Message = entity.Message,
                Email = entity.Email,
                VisitDate = entity.VisitDate,
                ClientCode = entity.ClientCode,
                PhotoUrl = entity.PhotoUrl,
                Status = entity.Status,
                CreateAt = entity.CreateAt
            };
        }

        // Se llama después de validar el DTO, así que portal y tipo ya sabemos que son válidos.
        public static Tourist ToEntity(this CreateTouristFeedbackDTO dto)
        {
            PortalSlugs.TryParse(dto.PortalId, out var portal);
            FeedbackKinds.TryParse(dto.Kind, out var kind);

            return new Tourist
            {
                Portal = portal,
                Kind = kind,
                Evaluation = dto.Rating,
                Message = dto.Message.Trim(),
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant(),
                VisitDate = DateOnly.TryParseExact(dto.VisitDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var visit)
                    ? visit
                    : null,
                ClientCode = string.IsNullOrWhiteSpace(dto.ClientCode) ? null : dto.ClientCode.Trim(),
                CreateBy = "Tourist"
            };
        }
    }
}
