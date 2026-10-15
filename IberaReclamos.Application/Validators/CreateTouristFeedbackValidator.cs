using System.Globalization;
using FluentValidation;
using IberaReclamos.Application.Common;
using IberaReclamos.Application.DTOs.Tourist;

namespace IberaReclamos.Application.Validators
{
    // Mismas reglas que feedbackInputSchema (zod) del frontend de turistas, con los mismos mensajes,
    // para que el backend nunca acepte algo que el formulario rechaza (y viceversa).
    public class CreateTouristFeedbackValidator : AbstractValidator<CreateTouristFeedbackDTO>
    {
        public const int MessageMinLength = 10;
        public const int MessageMaxLength = 1000;

        public CreateTouristFeedbackValidator()
        {
            RuleFor(x => x.PortalId)
                .Must(p => PortalSlugs.TryParse(p, out _))
                .WithMessage("Elegí el portal que visitaste.");

            RuleFor(x => x.Kind)
                .Must(k => FeedbackKinds.TryParse(k, out _))
                .WithMessage("Elegí qué querés contarnos.");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Elegí de 1 a 5 estrellas.");

            RuleFor(x => x.Message)
                .Must(m => (m?.Trim().Length ?? 0) >= MessageMinLength)
                    .WithMessage($"Contanos un poco más (al menos {MessageMinLength} caracteres).")
                .Must(m => (m?.Trim().Length ?? 0) <= MessageMaxLength)
                    .WithMessage($"El mensaje puede tener hasta {MessageMaxLength} caracteres.");

            RuleFor(x => x.VisitDate)
                .Must(BeAValidVisitDate)
                .When(x => !string.IsNullOrWhiteSpace(x.VisitDate))
                .WithMessage("La fecha tiene que ser de este último año y no futura.");

            RuleFor(x => x.Email!)
                .MaximumLength(150).WithMessage("Revisá el email.")
                .EmailAddress().WithMessage("Revisá el email.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.ClientCode)
                .MaximumLength(20).WithMessage("El código de la valoración no es válido.");
        }

        private static bool BeAValidVisitDate(string? value)
        {
            if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return false;

            // La fecha es la del turista (hora local). Damos un día de margen por diferencias de zona horaria.
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return date <= today.AddDays(1) && date >= today.AddYears(-1).AddDays(-1);
        }
    }
}
