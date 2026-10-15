using FluentValidation;
using IberaReclamos.Application.DTOs.Report;

namespace IberaReclamos.Application.Validators
{
    public class SyncReportsValidator : AbstractValidator<SyncReportsDTO>
    {
        // Tope por request para que una sincronización gigante no bloquee la API; la app manda en tandas.
        public const int MaxReportsPerSync = 200;

        public SyncReportsValidator()
        {
            RuleFor(x => x.Reports)
                .NotEmpty().WithMessage("No hay reportes para sincronizar.")
                .Must(r => r.Count <= MaxReportsPerSync)
                    .WithMessage($"Se pueden sincronizar hasta {MaxReportsPerSync} reportes por vez.");

            // En la sincronización el ClientId es obligatorio: es lo que evita duplicados si se reintenta.
            RuleForEach(x => x.Reports)
                .SetValidator(new CreateReportValidator())
                .ChildRules(report =>
                {
                    report.RuleFor(r => r.ClientId)
                        .NotEmpty().WithMessage("Cada reporte sincronizado debe traer su ClientId.");
                });

            RuleFor(x => x.Reports)
                .Must(r => r.Where(x => x.ClientId.HasValue).GroupBy(x => x.ClientId).All(g => g.Count() == 1))
                .WithMessage("Hay reportes repetidos (mismo ClientId) en la misma sincronización.");
        }
    }
}
