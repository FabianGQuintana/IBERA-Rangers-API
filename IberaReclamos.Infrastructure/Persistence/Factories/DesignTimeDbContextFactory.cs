using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IberaReclamos.Infrastructure.Persistence.Factories
{
    // La usa SOLO la herramienta "dotnet ef" (migraciones) para crear el DbContext sin levantar la API.
    // Toma la cadena de la variable de entorno ConnectionStrings__DefaultConnection;
    // si no existe usa una local de ejemplo (alcanza para "migrations add", que no se conecta a la DB).
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IberaReclamosDbContext>
    {
        public IberaReclamosDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=ibera_reclamos;Username=postgres;Password=postgres";

            var options = new DbContextOptionsBuilder<IberaReclamosDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new IberaReclamosDbContext(options);
        }
    }
}
