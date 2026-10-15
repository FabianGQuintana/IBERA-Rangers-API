namespace IberaReclamos.Application.Common.Options
{
    /// <summary>
    /// Configuración del JWT (sección "Jwt"). La Key es un secreto: va en User Secrets o en la
    /// variable de entorno Jwt__Key, nunca en appsettings.json.
    /// </summary>
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        // Mínimo 32 caracteres (256 bits) para HMAC-SHA256
        public string Key { get; set; } = string.Empty;

        public string Issuer { get; set; } = "IberaReclamos.API";

        public string Audience { get; set; } = "IberaReclamos.Clients";

        // Los rangers trabajan offline muchas horas: un token de 12 h evita que se deslogueen en el terreno.
        public int DurationInMinutes { get; set; } = 720;
    }
}
