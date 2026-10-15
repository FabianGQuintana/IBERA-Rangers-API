using IberaReclamos.Application.DTOs.User;
using IberaReclamos.Application.Interfaces.Common;
using IberaReclamos.Application.Interfaces.Services;
using IberaReclamos.Application.Mappers;
using IberaReclamos.Domain.Exceptions;
using IberaReclamos.Domain.Interfaces;

namespace IberaReclamos.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<UserResponseDTO>> GetAllAsync()
        {
            var users = await _unitOfWork.Users.GetAllWithRoleAsync();
            return users.Select(u => u.ToDto());
        }

        public async Task<UserResponseDTO> GetByIdAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdWithRoleAsync(id)
                ?? throw new NotFoundException("El usuario no existe.");

            return user.ToDto();
        }

        public async Task<UserResponseDTO> CreateAsync(CreateUserDTO dto, string createdBy)
        {
            // 1. Reglas de negocio que el validador no puede chequear (necesitan la DB)
            if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email))
                throw new ConflictException("El email ya está registrado.");

            if (await _unitOfWork.Users.ExistsByDniAsync(dto.Dni))
                throw new ConflictException("El DNI ya está registrado.");

            if (!await _unitOfWork.Roles.AnyAsync(r => r.ID == dto.IdRol))
                throw new BadRequestException("El rol seleccionado no existe.");

            // 2. Hasheamos la contraseña con BCrypt (genera su propio "salt" en cada hash)
            var passwordHash = _passwordHasher.Hash(dto.Password);

            // 3. Mapeamos y guardamos
            var user = dto.ToEntity(passwordHash);
            user.CreateBy = createdBy;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Volvemos a leerlo con el Rol incluido para devolver el nombre del rol
            return await GetByIdAsync(user.ID);
        }

        public async Task<UserResponseDTO> UpdateAsync(Guid id, UpdateUserDTO dto, string modifiedBy)
        {
            var user = await _unitOfWork.Users.GetByIdWithRoleAsync(id)
                ?? throw new NotFoundException("El usuario no existe.");

            if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email, excludeId: id))
                throw new ConflictException("El email ya está registrado.");

            if (user.IdRol != dto.IdRol && !await _unitOfWork.Roles.AnyAsync(r => r.ID == dto.IdRol))
                throw new BadRequestException("El rol seleccionado no existe.");

            user.ApplyUpdate(dto);
            user.LastModifiedBy = modifiedBy;

            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDTO dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw new NotFoundException("El usuario no existe.");

            // BCrypt.Verify vuelve a hashear la contraseña ingresada con el salt guardado y compara
            if (!_passwordHasher.Verify(dto.CurrentPassword, user.Password_hash))
                throw new BadRequestException("La contraseña actual no es correcta.");

            user.Password_hash = _passwordHasher.Hash(dto.NewPassword);
            user.LastModifiedBy = userId.ToString();

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<UserResponseDTO> ToggleActiveAsync(Guid id, Guid requestedBy)
        {
            if (id == requestedBy)
                throw new BadRequestException("No podés desactivar tu propio usuario.");

            var user = await _unitOfWork.Users.SoftDeleteToggleAsync(id)
                ?? throw new NotFoundException("El usuario no existe.");

            user.LastModifiedBy = requestedBy.ToString();
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }
    }
}
