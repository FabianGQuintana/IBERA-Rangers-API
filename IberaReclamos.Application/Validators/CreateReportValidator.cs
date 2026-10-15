using FluentValidation;
using IberaReclamos.Application.Common;
using IberaReclamos.Application.DTOs.Report;

namespace IberaReclamos.Application.Validators
{
    public class CreateReportValidator : AbstractValidator<CreateReportDTO>
    {
        public CreateReportValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("La latitud debe estar entre -90 y 90.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("La longitud debe estar entre -180 y 180.");

            // 0,0 es "en el medio del océano": casi siempre significa que el GPS no tomó posición.
            RuleFor(x => x)
                .Must(x => !(x.Latitude == 0 && x.Longitude == 0))
                .WithName("Coordinates")
                .WithMessage("Las coordenadas 0,0 no son válidas: revisá que el GPS haya tomado la posición.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Las notas no pueden superar los 1000 caracteres.");

            // Un reporte offline puede tener días, pero nunca puede ser del futuro
            // (dejamos 5 minutos de margen por relojes de celular desfasados).
            RuleFor(x => x.CaptureDate)
                .NotEmpty().WithMessage("La fecha de captura es obligatoria.")
                .Must(d => DateHelper.ToUtc(d) <= DateTime.UtcNow.AddMinutes(5))
                    .WithMessage("La fecha de captura no puede ser futura.")
                .Must(d => DateHelper.ToUtc(d) >= DateTime.UtcNow.AddYears(-1))
                    .WithMessage("La fecha de captura no puede tener más de un año.");

            RuleFor(x => x.ReportCategoryId)
                .NotEmpty().WithMessage("La categoría del reporte es obligatoria.");

            RuleFor(x => x.ClientId)
                .Must(id => id != Guid.Empty).When(x => x.ClientId.HasValue)
                .WithMessage("El ClientId no puede ser un Guid vacío.");
        }
    }
}
