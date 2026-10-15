namespace IberaReclamos.Domain.Constants
{
    /// <summary>
    /// Códigos de error públicos y estables del contrato de la API (el frontend puede decidir por código).
    /// No exponer mensajes internos de la DB ni stack traces.
    /// </summary>
    public static class ErrorCodes
    {
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string NotFound = "NOT_FOUND";
        public const string Conflict = "CONFLICT";
        public const string PayloadTooLarge = "PAYLOAD_TOO_LARGE";
        public const string TooManyRequests = "TOO_MANY_REQUESTS";
        public const string InternalError = "INTERNAL_ERROR";
    }
}
