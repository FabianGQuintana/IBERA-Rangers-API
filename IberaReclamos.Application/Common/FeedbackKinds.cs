using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Application.Common
{
    // "review" | "suggestion" | "complaint" (frontend) <-> FeedbackKind (dominio)
    public static class FeedbackKinds
    {
        public static bool TryParse(string? value, out FeedbackKind kind)
        {
            kind = default;
            return !string.IsNullOrWhiteSpace(value)
                && !value.Trim().All(char.IsDigit)
                && Enum.TryParse(value.Trim(), ignoreCase: true, out kind)
                && Enum.IsDefined(kind);
        }

        public static string ToSlug(FeedbackKind kind) => kind.ToString().ToLowerInvariant();
    }
}
