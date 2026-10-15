using IberaReclamos.Application.DTOs.User;

namespace IberaReclamos.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDTO>> GetAllAsync();

        Task<UserResponseDTO> GetByIdAsync(Guid id);

        Task<UserResponseDTO> CreateAsync(CreateUserDTO dto, string createdBy);

        Task<UserResponseDTO> UpdateAsync(Guid id, UpdateUserDTO dto, string modifiedBy);

        Task ChangePasswordAsync(Guid userId, ChangePasswordDTO dto);

        // Activa/desactiva (borrado lógico). Un usuario desactivado no puede loguearse.
        Task<UserResponseDTO> ToggleActiveAsync(Guid id, Guid requestedBy);
    }
}
