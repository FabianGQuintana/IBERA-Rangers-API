using FluentValidation;
using IberaReclamos.Application.DTOs.User;

namespace IberaReclamos.Application.Validators
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserDTO>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es obligatorio.")
                .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("El número de teléfono es obligatorio.")
                .Matches(ValidationRules.PhonePattern).WithMessage("El número de teléfono no es válido.");

            RuleFor(x => x.Email)
                .ValidEmail();

            RuleFor(x => x.Portal)
                .IsInEnum().WithMessage("El portal seleccionado no es válido.");

            RuleFor(x => x.IdRol)
                .NotEmpty().WithMessage("El rol es obligatorio.");
        }
    }
}
