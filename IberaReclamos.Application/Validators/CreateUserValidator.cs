using FluentValidation;
using IberaReclamos.Application.DTOs.User;

namespace IberaReclamos.Application.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserDTO>
    {
        public CreateUserValidator()
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

            // El DNI es string (puede empezar con 0 y no hacemos cuentas con él),
            // por eso se valida con una expresión regular y no con InclusiveBetween.
            RuleFor(x => x.Dni)
                .NotEmpty().WithMessage("El DNI es obligatorio.")
                .Matches(ValidationRules.DniPattern).WithMessage("El DNI ingresado no es válido. Debe contener entre 7 y 8 dígitos, sin puntos.");

            RuleFor(x => x.Portal)
                .IsInEnum().WithMessage("El portal seleccionado no es válido.");

            RuleFor(x => x.Password)
                .StrongPassword();

            RuleFor(x => x.IdRol)
                .NotEmpty().WithMessage("El rol es obligatorio.");
        }
    }
}
