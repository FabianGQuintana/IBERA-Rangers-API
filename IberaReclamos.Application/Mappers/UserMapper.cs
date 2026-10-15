using IberaReclamos.Application.DTOs.User;
using IberaReclamos.Domain.Entities;

namespace IberaReclamos.Application.Mappers
{
    // Mappers manuales: convierten Entidad <-> DTO sin librerías (AutoMapper, etc.).
    public static class UserMapper
    {
        public static UserResponseDTO ToDto(this User entity)
        {
            return new UserResponseDTO
            {
                Id = entity.ID,
                Name = entity.Name,
                LastName = entity.LastName,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                Dni = entity.Dni,
                Portal = entity.Portal.ToString(),
                IdRol = entity.IdRol,
                // Rol puede venir null si no se hizo Include(u => u.Rol)
                Role = entity.Rol?.RolName.ToString() ?? string.Empty,
                IsActive = !entity.IsDeleted,
                CreateAt = entity.CreateAt
            };
        }

        // El hash lo calcula el servicio (BCrypt) y nos lo pasa: el mapper nunca ve la contraseña en texto plano.
        public static User ToEntity(this CreateUserDTO dto, string passwordHash)
        {
            return new User
            {
                Name = dto.Name.Trim(),
                LastName = dto.LastName.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                Dni = dto.Dni.Trim(),
                Portal = dto.Portal,
                Password_hash = passwordHash,
                IdRol = dto.IdRol
            };
        }

        public static void ApplyUpdate(this User entity, UpdateUserDTO dto)
        {
            entity.Name = dto.Name.Trim();
            entity.LastName = dto.LastName.Trim();
            entity.PhoneNumber = dto.PhoneNumber.Trim();
            entity.Email = dto.Email.Trim().ToLowerInvariant();
            entity.Portal = dto.Portal;
            entity.IdRol = dto.IdRol;
        }
    }
}
