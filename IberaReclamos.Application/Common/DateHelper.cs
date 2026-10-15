namespace IberaReclamos.Application.Common
{
    public static class DateHelper
    {
        // PostgreSQL (timestamp with time zone) solo acepta fechas UTC.
        // Si la fecha llega sin zona (Kind = Unspecified) la tomamos como UTC; si viene en hora local, la convertimos.
        public static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        public static DateTime? ToUtc(DateTime? value) => value.HasValue ? ToUtc(value.Value) : null;
    }
}
