using IberaReclamos.Application.DTOs.ReportCategory;

namespace IberaReclamos.Application.Interfaces.Services
{
    public interface IReportCategoryService
    {
        Task<IEnumerable<ReportCategoryResponseDTO>> GetAllAsync();

        Task<ReportCategoryResponseDTO> CreateAsync(CreateReportCategoryDTO dto, string createdBy);
    }
}
