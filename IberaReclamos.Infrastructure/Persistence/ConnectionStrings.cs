using Npgsql;

namespace IberaReclamos.Infrastructure.Persistence
{
    public static class ConnectionStrings
    {
        /// <summary>
        /// Convierte una URL de Postgres (formato de Railway, Render, Heroku: postgresql://user:pass@host:port/db)
        /// al formato que entiende Npgsql (Host=...;Port=...;Database=...). Devuelve null si no hay URL.
        /// </summary>
        public static string? FromDatabaseUrl(string? databaseUrl)
        {
            if (string.IsNullOrWhiteSpace(databaseUrl)) return null;
            if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri)
                || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
            {
                // Ya viene en formato Npgsql ("Host=...")
                return databaseUrl;
            }

            var userInfo = uri.UserInfo.Split(':', 2);
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.AbsolutePath.TrimStart('/'),
                Username = Uri.UnescapeDataString(userInfo[0]),
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
                // La red privada de Railway no usa SSL; la conexión pública sí. Prefer sirve para ambas.
                SslMode = SslMode.Prefer
            };
            return builder.ConnectionString;
        }
    }
}
