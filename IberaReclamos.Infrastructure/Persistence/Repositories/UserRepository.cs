using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IberaReclamos.Infrastructure.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(IberaReclamosDbContext context) : base(context) { }

        public async Task<User?> GetByEmailWithRoleAsync(string email)
        {
            var normalized = email.Trim().ToLowerInvariant();
            return await _dbSet
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == normalized);
        }

        public async Task<User?> GetByIdWithRoleAsync(Guid id)
        {
            return await _dbSet
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.ID == id);
        }

        public async Task<IEnumerable<User>> GetAllWithRoleAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(u => u.Rol)
                .OrderBy(u => u.LastName).ThenBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null)
        {
            var normalized = email.Trim().ToLowerInvariant();
            return await _dbSet.AnyAsync(u => u.Email == normalized && (excludeId == null || u.ID != excludeId));
        }

        public async Task<bool> ExistsByDniAsync(string dni)
        {
            var normalized = dni.Trim();
            return await _dbSet.AnyAsync(u => u.Dni == normalized);
        }
    }
}
