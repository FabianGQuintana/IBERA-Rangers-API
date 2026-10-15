using IberaReclamos.Application.DTOs.ReportCategory;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Mappers;
using IberaReclamos.Domain.Exceptions;
using IberaReclamos.Domain.Interfaces;

namespace IberaReclamos.Application.Services
{
    public class ReportCategoryService : IReportCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportCategoryService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<IEnumerable<ReportCategoryResponseDTO>> GetAllAsync()
        {
            var categories = await _unitOfWork.ReportCategories.WhereAsync(c => !c.IsDeleted);
            return categories.OrderBy(c => c.Name).Select(c => c.ToDto());
        }

        public async Task<ReportCategoryResponseDTO> CreateAsync(CreateReportCategoryDTO dto, string createdBy)
        {
            var name = dto.Name.Trim();
            if (await _unitOfWork.ReportCategories.AnyAsync(c => c.Name.ToLower() == name.ToLower()))
                throw new ConflictException("Ya existe una categoría con ese nombre.");

            var category = dto.ToEntity();
            category.CreateBy = createdBy;

            await _unitOfWork.ReportCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category.ToDto();
        }
    }
}
