using IberaReclamos.Application.DTOs.ReportCategory;
using IberaReclamos.Domain.Entities;

namespace IberaReclamos.Application.Mappers
{
    public static class ReportCategoryMapper
    {
        public static ReportCategoryResponseDTO ToDto(this ReportCategory entity)
        {
            return new ReportCategoryResponseDTO
            {
                Id = entity.ID,
                Name = entity.Name,
                Description = entity.Description,
                SeverityLevel = entity.SeverityLevel.ToString()
            };
        }

        public static ReportCategory ToEntity(this CreateReportCategoryDTO dto)
        {
            return new ReportCategory
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                SeverityLevel = dto.SeverityLevel
            };
        }
    }
}
