namespace IberaReclamos.Application.Common.Options
{
    /// <summary>
    /// Configuración de Cloudflare R2 (sección "R2"). R2 es compatible con la API de Amazon S3,
    /// por eso usamos el SDK de AWS apuntando al endpoint de Cloudflare.
    /// Las credenciales van en User Secrets / variables de entorno (R2__AccessKeyId, R2__SecretAccessKey...).
    /// </summary>
    public class R2Options
    {
        public const string SectionName = "R2";

        // Si es false (o faltan datos) la API guarda las fotos en disco local (wwwroot/uploads) para desarrollo.
        public bool Enabled { get; set; }

        public string AccountId { get; set; } = string.Empty;

        public string AccessKeyId { get; set; } = string.Empty;

        public string SecretAccessKey { get; set; } = string.Empty;

        public string BucketName { get; set; } = string.Empty;

        // Dominio público del bucket (r2.dev o dominio propio), sin "/" final. Ej: https://media.ibera.example
        public string PublicBaseUrl { get; set; } = string.Empty;

        // Endpoint S3 de la cuenta: https://<account-id>.r2.cloudflarestorage.com
        public string ServiceUrl => $"https://{AccountId}.r2.cloudflarestorage.com";

        public bool IsConfigured =>
            Enabled
            && !string.IsNullOrWhiteSpace(AccountId)
            && !string.IsNullOrWhiteSpace(AccessKeyId)
            && !string.IsNullOrWhiteSpace(SecretAccessKey)
            && !string.IsNullOrWhiteSpace(BucketName)
            && !string.IsNullOrWhiteSpace(PublicBaseUrl);
    }
}
