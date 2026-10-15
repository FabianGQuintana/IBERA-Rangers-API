using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IberaReclamos.Infrastructure.Persistence.Factories
{
    // La usa SOLO la herramienta "dotnet ef" (migrations add, database update) para crear el DbContext
    // sin levantar toda la API (así no hace falta configurar Jwt, R2, etc. para migrar).
    // Lee la cadena de conexión del MISMO lugar que la API:
    //   1. User Secrets del proyecto API (dotnet user-secrets, en desarrollo)
    //   2. Variable de entorno ConnectionStrings__DefaultConnection (gana si existe, ej: en producción)
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IberaReclamosDbContext>
    {
        // Debe coincidir con <UserSecretsId> de IberaReclamos.API.csproj
        private const string ApiUserSecretsId = "3f6c1e2a-7b4d-4c1e-9a8f-1b2c3d4e5f60";

        public IberaReclamosDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(ApiUserSecretsId)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection is not configured. Run from IberaReclamos.API: " +
                    "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"Host=localhost;Port=5432;Database=...;Username=postgres;Password=...\"");
            }

            var options = new DbContextOptionsBuilder<IberaReclamosDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new IberaReclamosDbContext(options);
        }
    }
}
