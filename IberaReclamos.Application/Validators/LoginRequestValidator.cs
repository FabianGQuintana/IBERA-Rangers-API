using FluentValidation;
using IberaReclamos.Application.DTOs.Auth;

namespace IberaReclamos.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDTO>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .ValidEmail();

            // En el login no validamos la fortaleza: solo que venga algo.
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.");
        }
    }
}
