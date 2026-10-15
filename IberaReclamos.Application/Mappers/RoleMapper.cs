using IberaReclamos.Application.DTOs.Role;
using IberaReclamos.Domain.Entities;

namespace IberaReclamos.Application.Mappers
{
    public static class RoleMapper
    {
        public static RoleResponseDTO ToDto(this Role entity)
        {
            return new RoleResponseDTO
            {
                Id = entity.ID,
                Name = entity.RolName.ToString(),
                Description = entity.Description
            };
        }
    }
}
