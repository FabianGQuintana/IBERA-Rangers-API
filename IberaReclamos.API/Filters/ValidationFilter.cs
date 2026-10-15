using FluentValidation;
using IberaReclamos.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IberaReclamos.API.Filters
{
    /// <summary>
    /// Filtro global: antes de entrar a cualquier acción de un controller busca si existe un
    /// IValidator&lt;T&gt; (FluentValidation) para cada parámetro que llega (DTOs del body o del form)
    /// y, si algo no pasa la validación, corta el request con un 400 y los errores por campo.
    /// Así los controllers y servicios reciben siempre datos ya validados.
    /// </summary>
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Errores de "binding": JSON mal formado, un Guid inválido, un enum que no existe, etc.
            if (!context.ModelState.IsValid)
            {
                var bindingErrors = context.ModelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kv => ToCamelCase(kv.Key.TrimStart('$', '.')),
                        kv => kv.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Valor inválido." : e.ErrorMessage).ToArray());

                context.Result = new BadRequestObjectResult(new ApiErrorResponse
                {
                    Code = "VALIDATION_FAILED",
                    Message = "La solicitud tiene datos con formato inválido.",
                    TraceId = context.HttpContext.TraceIdentifier,
                    Errors = bindingErrors
                });
                return;
            }

            // 2. Reglas de negocio de FluentValidation
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                if (_serviceProvider.GetService(validatorType) is not IValidator validator) continue;

                var result = await validator.ValidateAsync(
                    new ValidationContext<object>(argument),
                    context.HttpContext.RequestAborted);

                if (result.IsValid) continue;

                var errors = result.Errors
                    .GroupBy(e => ToCamelCase(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

                context.Result = new BadRequestObjectResult(new ApiErrorResponse
                {
                    Code = "VALIDATION_FAILED",
                    // El primer mensaje sirve para mostrarlo directo (el front de turistas lee "message").
                    Message = result.Errors[0].ErrorMessage,
                    TraceId = context.HttpContext.TraceIdentifier,
                    Errors = errors
                });
                return;
            }

            await next();
        }

        private static string ToCamelCase(string propertyName) =>
            string.IsNullOrEmpty(propertyName) ? propertyName : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
    }
}
