using IberaReclamos.Domain.Entities;

namespace IberaReclamos.Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailWithRoleAsync(string email);

        Task<User?> GetByIdWithRoleAsync(Guid id);

        Task<IEnumerable<User>> GetAllWithRoleAsync();

        // excludeId: al editar un usuario, su propio email no cuenta como repetido
        Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null);

        Task<bool> ExistsByDniAsync(string dni);
    }
}
