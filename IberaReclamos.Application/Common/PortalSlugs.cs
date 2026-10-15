using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Application.Common
{
    // Traduce los ids de portal que usa el frontend de turistas ("laguna", "sannicolas", ...)
    // al enum IberaPortales del dominio, y viceversa.
    public static class PortalSlugs
    {
        private static readonly Dictionary<string, IberaPortales> SlugToPortal = new(StringComparer.OrdinalIgnoreCase)
        {
            ["sannicolas"] = IberaPortales.SanNicolas,
            ["laguna"] = IberaPortales.CarlosPellegrini,
            ["carambola"] = IberaPortales.Carambola,
            ["cambyreta"] = IberaPortales.Cambyreta,
            ["sanantonio"] = IberaPortales.SanAntonio,
            ["galarza"] = IberaPortales.Galarza,
            ["uguay"] = IberaPortales.Uguay,
            ["riocorriente"] = IberaPortales.RioCorriente,
            ["capivari"] = IberaPortales.Capivari,
            ["mburucuya"] = IberaPortales.Mburucuya,
        };

        public static bool TryParse(string? value, out IberaPortales portal)
        {
            portal = default;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var trimmed = value.Trim();
            if (SlugToPortal.TryGetValue(trimmed, out portal)) return true;

            // También aceptamos el nombre del enum ("CarlosPellegrini"), pero no números.
            return !trimmed.All(char.IsDigit)
                && Enum.TryParse(trimmed, ignoreCase: true, out portal)
                && Enum.IsDefined(portal);
        }

        public static string ToSlug(IberaPortales portal) =>
            SlugToPortal.FirstOrDefault(kv => kv.Value == portal).Key ?? portal.ToString().ToLowerInvariant();
    }
}
