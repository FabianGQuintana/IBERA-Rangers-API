namespace IberaReclamos.Application.DTOs.Tourist
{
    // Contrato EXACTO del formulario público del frontend de turistas (POST /api/feedback, multipart/form-data).
    // Los nombres coinciden con los campos que manda services/feedback.ts. La foto viaja como archivo aparte.
    public class CreateTouristFeedbackDTO
    {
        // Id del portal en el frontend: "laguna", "cambyreta", "sannicolas", ...
        public string PortalId { get; set; } = string.Empty;

        // "review" | "suggestion" | "complaint"
        public string Kind { get; set; } = string.Empty;

        // Estrellas de 1 a 5
        public int Rating { get; set; }

        public string Message { get; set; } = string.Empty;

        // Fecha local del turista en formato YYYY-MM-DD (opcional)
        public string? VisitDate { get; set; }

        public string? Email { get; set; }

        // Código generado por el frontend (IB-V1234) para no duplicar envíos de la cola offline.
        public string? ClientCode { get; set; }

        // Honeypot: un humano nunca lo completa; si viene con algo, es un bot.
        public string? Website { get; set; }
    }
}
