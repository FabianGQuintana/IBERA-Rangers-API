using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Domain.Entities
{
    // Valoración/queja que deja un turista desde la web pública (sin cuenta).
    public class Tourist : BaseEntity
    {
        public required string Message { get; set; }

        // Estrellas de 1 a 5.
        public required int Evaluation { get; set; }

        public required FeedbackKind Kind { get; set; }

        public string? Email { get; set; }

        public required IberaPortales Portal { get; set; }

        public DateOnly? VisitDate { get; set; }

        // Código generado por el frontend (ej: IB-V1234). Si el turista estaba sin señal,
        // la valoración se reenvía y con este código evitamos guardarla dos veces.
        public string? ClientCode { get; set; }

        // Foto opcional en Cloudflare R2 (solo guardamos la referencia).
        public string? PhotoKey { get; set; }

        public string? PhotoUrl { get; set; }
    }
}
