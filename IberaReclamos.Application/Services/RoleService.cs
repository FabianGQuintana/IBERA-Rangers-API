using IberaReclamos.Application.DTOs.Role;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Mappers;
using IberaReclamos.Domain.Interfaces;

namespace IberaReclamos.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<IEnumerable<RoleResponseDTO>> GetAllAsync()
        {
            var roles = await _unitOfWork.Roles.GetAllAsync();
            return roles.OrderBy(r => r.RolName).Select(r => r.ToDto());
        }
    }
}
