using IberaReclamos.Application.DTOs.Role;

namespace IberaReclamos.Application.Interfaces.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponseDTO>> GetAllAsync();
    }
}
