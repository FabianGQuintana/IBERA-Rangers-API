using IberaReclamos.Application.DTOs.Auth;
using IberaReclamos.Application.DTOs.User;

namespace IberaReclamos.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto);

        Task<UserResponseDTO> GetMeAsync(Guid userId);
    }
}
