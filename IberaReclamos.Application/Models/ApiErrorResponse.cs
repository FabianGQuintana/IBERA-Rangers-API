namespace IberaReclamos.Application.Models
{
    /// <summary>
    /// Contrato público de error de la API (lo usan el filtro de validación y el ExceptionMiddleware).
    /// Nunca incluir detail/inner/stackTrace.
    /// </summary>
    public sealed class ApiErrorResponse
    {
        public required string Code { get; init; }

        public required string Message { get; init; }

        public string? TraceId { get; init; }

        // Errores de validación por campo: { "email": ["Revisá el email."] }
        public Dictionary<string, string[]>? Errors { get; init; }
    }
}
