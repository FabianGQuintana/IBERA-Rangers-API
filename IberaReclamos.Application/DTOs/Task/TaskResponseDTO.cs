namespace IberaReclamos.Application.DTOs.Task
{
    public class TaskResponseDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        public Guid AssignedById { get; set; }

        public string AssignedBy { get; set; } = string.Empty;

        public Guid AssignedToId { get; set; }

        public string AssignedTo { get; set; } = string.Empty;

        public Guid? ReportId { get; set; }

        public DateTime CreateAt { get; set; }
    }
}
