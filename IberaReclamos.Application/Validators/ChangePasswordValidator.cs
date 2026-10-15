using FluentValidation;
using IberaReclamos.Application.DTOs.User;

namespace IberaReclamos.Application.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDTO>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("La contraseña actual es obligatoria.");

            RuleFor(x => x.NewPassword)
                .StrongPassword()
                .NotEqual(x => x.CurrentPassword).WithMessage("La nueva contraseña tiene que ser distinta de la actual.");
        }
    }
}
