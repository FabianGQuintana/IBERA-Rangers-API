using FluentValidation;
using IberaReclamos.Application.DTOs.ReportCategory;

namespace IberaReclamos.Application.Validators
{
    public class CreateReportCategoryValidator : AbstractValidator<CreateReportCategoryDTO>
    {
        public CreateReportCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.");

            RuleFor(x => x.SeverityLevel)
                .IsInEnum().WithMessage("El nivel de gravedad no es válido.");
        }
    }
}
