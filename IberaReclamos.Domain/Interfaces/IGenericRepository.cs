using System.Linq.Expressions;
using IberaReclamos.Domain.Entities;

namespace IberaReclamos.Domain.Interfaces
{
    //Esta interfaz sirve como base para todos los repositorios específicos
    //de cada entidad (User, Report, etc.). Define los métodos comunes a todos ellos.
    //IMPORTANTE: ningún método guarda en la DB; el guardado real lo hace IUnitOfWork.SaveChangesAsync().
    public interface IGenericRepository<T> where T : BaseEntity
    {
        IQueryable<T> GetQueryable(); // Para armar consultas a medida desde un repositorio específico

        Task<T?> GetByIdAsync(Guid id); // Obtener una entidad por su ID

        Task<IEnumerable<T>> GetAllAsync(); // Todas las entidades (sin seguimiento de cambios)

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate); // Query filtrada en SQL (uno)

        Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate); // Query filtrada en SQL (lista)

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate); // ¿Existe alguno que cumpla la condición?

        Task<T> AddAsync(T entity); // Agregar una nueva entidad

        Task AddRangeAsync(IEnumerable<T> entities); // Agregar muchas de una vez (bulk insert)

        Task<T> UpdateAsync(T entity); // Actualizar una entidad existente

        Task<T?> SoftDeleteToggleAsync(Guid id); // Borrado lógico: si está activa la borra, si está borrada la restaura

        Task RemoveRangeAsync(IEnumerable<T> entities); // Eliminación física en lote
    }
}
