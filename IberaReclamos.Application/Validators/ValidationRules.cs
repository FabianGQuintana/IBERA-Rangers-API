using FluentValidation;

namespace IberaReclamos.Application.Validators
{
    // Reglas reutilizables entre varios validadores (para no copiar y pegar la misma regla).
    public static class ValidationRules
    {
        public const string DniPattern = @"^\d{7,8}$";
        public const string PhonePattern = @"^\+?[0-9\s\-()]{6,25}$";

        public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .MaximumLength(72).WithMessage("La contraseña no puede superar los 72 caracteres.") // límite de BCrypt
                .Matches("[A-Za-z]").WithMessage("La contraseña debe tener al menos una letra.")
                .Matches("[0-9]").WithMessage("La contraseña debe tener al menos un número.");
        }

        public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("El email es obligatorio.")
                .MaximumLength(150).WithMessage("El email no puede superar los 150 caracteres.")
                .EmailAddress().WithMessage("El formato del email no es válido.");
        }
    }
}
