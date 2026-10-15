using FluentValidation;
using IberaReclamos.Application.DTOs.Report;
using IberaReclamos.Application.DTOs.Task;

namespace IberaReclamos.Application.Validators
{
    public class UpdateReportStatusValidator : AbstractValidator<UpdateReportStatusDTO>
    {
        public UpdateReportStatusValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("El estado del reporte no es válido.");
        }
    }

    public class UpdateTaskStatusValidator : AbstractValidator<UpdateTaskStatusDTO>
    {
        public UpdateTaskStatusValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("El estado de la tarea no es válido.");
        }
    }
}
