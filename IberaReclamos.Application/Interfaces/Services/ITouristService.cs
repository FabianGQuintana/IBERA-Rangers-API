using IberaReclamos.Application.DTOs.Tourist;
using IberaReclamos.Application.Models;

namespace IberaReclamos.Application.Interfaces.Services
{
    public interface ITouristService
    {
        // Formulario público de valoraciones/quejas (sin login)
        Task<FeedbackCreatedDTO> SubmitFeedbackAsync(CreateTouristFeedbackDTO dto, FileUpload? photo, CancellationToken cancellationToken = default);

        // Panel interno: listado con filtros opcionales (portal = slug del front o nombre del enum; kind = review|suggestion|complaint)
        Task<IEnumerable<TouristResponseDTO>> GetFilteredAsync(string? portal, string? kind);

        Task<TouristResponseDTO> GetByIdAsync(Guid id);
    }
}
