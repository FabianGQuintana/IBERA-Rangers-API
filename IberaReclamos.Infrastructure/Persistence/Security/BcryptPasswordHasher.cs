using IberaReclamos.Application.Interfaces.Common;

namespace IberaReclamos.Infrastructure.Persistence.Security
{
    // Implementación de IPasswordHasher con BCrypt.Net-Next.
    // BCrypt es lento a propósito (WorkFactor) y genera un "salt" aleatorio por cada hash,
    // así dos usuarios con la misma contraseña tienen hashes distintos.
    public class BcryptPasswordHasher : IPasswordHasher
    {
        // 2^11 rondas: buen equilibrio entre seguridad y tiempo de respuesta del login.
        private const int WorkFactor = 11;

        public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

        public bool Verify(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // El hash guardado no tiene formato BCrypt: lo tratamos como contraseña incorrecta
                return false;
            }
        }
    }
}
