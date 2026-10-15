namespace IberaReclamos.Domain.Interfaces
{
    // Transacción explícita: todo o nada (se usa en la sincronización masiva de reportes).
    public interface ITransaction : IAsyncDisposable
    {
        Task CommitAsync();

        Task RollbackAsync();
    }
}
