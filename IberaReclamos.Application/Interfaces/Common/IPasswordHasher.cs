namespace IberaReclamos.Application.Interfaces.Common
{
    // Contrato para hashear contraseñas. La implementación (BCrypt) vive en Infrastructure,
    // así Application no depende de ninguna librería concreta.
    public interface IPasswordHasher
    {
        string Hash(string password);

        bool Verify(string password, string hash);
    }
}
