using FluentValidation;
using IberaReclamos.Application.Common;
using IberaReclamos.Application.DTOs.Task;

namespace IberaReclamos.Application.Validators
{
    public class CreateTaskValidator : AbstractValidator<CreateTaskDTO>
    {
        public CreateTaskValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la tarea es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("La descripción no puede superar los 1000 caracteres.");

            RuleFor(x => x.AssignedToId)
                .NotEmpty().WithMessage("Hay que indicar a qué ranger se le asigna la tarea.");

            RuleFor(x => x.DueDate)
                .Must(d => DateHelper.ToUtc(d!.Value) > DateTime.UtcNow)
                .When(x => x.DueDate.HasValue)
                .WithMessage("La fecha límite tiene que ser futura.");
        }
    }
}
