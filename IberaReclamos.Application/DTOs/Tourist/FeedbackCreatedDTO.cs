namespace IberaReclamos.Application.DTOs.Tourist
{
    // Respuesta que espera el frontend: { "code": "IB-V1234" }
    public class FeedbackCreatedDTO
    {
        public string Code { get; set; } = string.Empty;
    }
}
