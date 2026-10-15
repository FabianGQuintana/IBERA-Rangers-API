using IberaReclamos.Application.Interfaces.Common;
using IberaReclamos.Domain.Entities;
using IberaReclamos.Domain.Enum;
using IberaReclamos.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IberaReclamos.Infrastructure.Persistence.Seed
{
    // Se ejecuta al arrancar la API:
    // 1. (opcional) aplica las migraciones pendientes
    // 2. crea el PRIMER administrador si todavía no hay ninguno (si no, nadie podría loguearse para crear usuarios)
    // Los datos del admin salen de User Secrets / variables de entorno (sección "SeedAdmin"), nunca del código.
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            IberaReclamosDbContext context,
            IPasswordHasher passwordHasher,
            IConfiguration configuration,
            ILogger logger)
        {
            if (bool.TryParse(configuration["Database:MigrateOnStartup"], out var migrate) && migrate)
            {
                logger.LogInformation("Applying pending migrations...");
                await context.Database.MigrateAsync();
            }

            var section = configuration.GetSection("SeedAdmin");
            var email = section["Email"];
            var password = section["Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return; // No hay admin configurado para crear

            var adminExists = await context.Users.AnyAsync(u => u.IdRol == RoleConfiguration.AdminRoleId);
            if (adminExists)
                return;

            if (password.Length < 8)
            {
                logger.LogWarning("SeedAdmin:Password must have at least 8 characters. Admin user was not created.");
                return;
            }

            context.Users.Add(new User
            {
                Name = section["Name"] ?? "Admin",
                LastName = section["LastName"] ?? "Iberá",
                PhoneNumber = section["PhoneNumber"] ?? "0000000",
                Email = email.Trim().ToLowerInvariant(),
                Dni = section["Dni"] ?? "10000000",
                Portal = IberaPortales.CarlosPellegrini,
                Password_hash = passwordHasher.Hash(password),
                IdRol = RoleConfiguration.AdminRoleId,
                CreateBy = "Seed"
            });

            await context.SaveChangesAsync();
            logger.LogInformation("Initial admin user created ({Email}).", email);
        }
    }
}
