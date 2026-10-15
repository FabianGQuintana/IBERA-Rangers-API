namespace IberaReclamos.Application.DTOs.Tourist
{
    public class TouristResponseDTO
    {
        public Guid Id { get; set; }

        public string Portal { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? Email { get; set; }

        public DateOnly? VisitDate { get; set; }

        public string? ClientCode { get; set; }

        public string? PhotoUrl { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; }
    }
}
