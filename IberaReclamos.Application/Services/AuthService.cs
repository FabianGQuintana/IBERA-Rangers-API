using IberaReclamos.Application.DTOs.Auth;
using IberaReclamos.Application.DTOs.User;
using IberaReclamos.Application.Interfaces.Common;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Mappers;
using IberaReclamos.Domain.Exceptions;
using IberaReclamos.Domain.Interfaces;

namespace IberaReclamos.Application.Services
{
    public class AuthService : IAuthService
    {
        // Mismo mensaje para "no existe" y "contraseña incorrecta": no le decimos a un atacante qué emails existen.
        private const string InvalidCredentials = "Email o contraseña incorrectos.";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto)
        {
            var user = await _unitOfWork.Users.GetByEmailWithRoleAsync(dto.Email);

            if (user is null || !_passwordHasher.Verify(dto.Password, user.Password_hash))
                throw new UnauthorizedException(InvalidCredentials);

            if (user.IsDeleted)
                throw new UnauthorizedException("Tu usuario está desactivado. Contactá a un administrador.");

            var token = _tokenService.GenerateToken(user);

            return new LoginResponseDTO
            {
                Token = token.Token,
                ExpiresAt = token.ExpiresAt,
                User = user.ToDto()
            };
        }

        public async Task<UserResponseDTO> GetMeAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdWithRoleAsync(userId)
                ?? throw new NotFoundException("El usuario no existe.");

            return user.ToDto();
        }
    }
}
