using System.Net;
using System.Text.Json;
using FluentValidation;
using IberaReclamos.Application.Models;
using IberaReclamos.Domain.Constants;
using IberaReclamos.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace IberaReclamos.API.Middleware
{
    /// <summary>
    /// Red de seguridad global: atrapa cualquier excepción que se escape de un controller/servicio
    /// y la convierte en una respuesta HTTP estandarizada (ApiErrorResponse), sin filtrar detalles
    /// internos (InnerException, mensajes de Npgsql, stack trace).
    /// </summary>
    public sealed class ExceptionMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // El cliente cortó la conexión (ej: el ranger perdió señal): no es un error del servidor
                _logger.LogInformation("Request aborted by the client. TraceId={TraceId}", context.TraceIdentifier);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = context.TraceIdentifier;
            var (statusCode, code, message, errors) = MapException(exception);

            if (statusCode >= (int)HttpStatusCode.InternalServerError)
                _logger.LogError(exception, "Unhandled exception. TraceId={TraceId}", traceId);
            else
                _logger.LogWarning("Handled exception {ErrorCode}: {Message}. TraceId={TraceId}", code, exception.Message, traceId);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started; cannot write error body. TraceId={TraceId}", traceId);
                throw exception;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var payload = new ApiErrorResponse
            {
                Code = code,
                Message = message,
                TraceId = traceId,
                Errors = errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }

        private static (int StatusCode, string Code, string Message, Dictionary<string, string[]>? Errors) MapException(Exception exception)
        {
            return exception switch
            {
                ValidationException ex => (
                    StatusCodes.Status400BadRequest,
                    ErrorCodes.ValidationFailed,
                    ex.Errors.FirstOrDefault()?.ErrorMessage ?? "Solicitud inválida.",
                    ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),

                BadRequestException ex => (StatusCodes.Status400BadRequest, ErrorCodes.ValidationFailed, ex.Message, null),

                UnauthorizedException ex => (StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized, ex.Message, null),

                ForbiddenException ex => (StatusCodes.Status403Forbidden, ErrorCodes.Forbidden, ex.Message, null),

                NotFoundException ex => (StatusCodes.Status404NotFound, ErrorCodes.NotFound, ex.Message, null),

                ConflictException ex => (StatusCodes.Status409Conflict, ErrorCodes.Conflict, ex.Message, null),

                // Ej: una foto más pesada que el límite de Kestrel
                BadHttpRequestException ex when ex.StatusCode == StatusCodes.Status413PayloadTooLarge => (
                    StatusCodes.Status413PayloadTooLarge,
                    ErrorCodes.PayloadTooLarge,
                    "El archivo es demasiado grande.",
                    null),

                BadHttpRequestException => (StatusCodes.Status400BadRequest, ErrorCodes.ValidationFailed, "Solicitud inválida.", null),

                // Dos requests iguales al mismo tiempo (ej: la misma sincronización enviada dos veces)
                DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) => (
                    StatusCodes.Status409Conflict,
                    ErrorCodes.Conflict,
                    "Ya existe un registro con esos datos.",
                    null),

                DbUpdateException => (
                    StatusCodes.Status409Conflict,
                    ErrorCodes.Conflict,
                    "No se pudo completar la operación por un conflicto de datos.",
                    null),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    ErrorCodes.InternalError,
                    "Ocurrió un error inesperado. Intente nuevamente más tarde.",
                    null)
            };
        }

        // 23505 = unique_violation en PostgreSQL
        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            for (var e = (Exception?)ex; e != null; e = e.InnerException)
            {
                if (e is Npgsql.PostgresException pg && pg.SqlState == Npgsql.PostgresErrorCodes.UniqueViolation)
                    return true;
            }

            return false;
        }
    }
}
