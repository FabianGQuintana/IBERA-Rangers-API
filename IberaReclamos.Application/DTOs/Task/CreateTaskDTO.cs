namespace IberaReclamos.Application.DTOs.Task
{
    public class CreateTaskDTO
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        // Ranger que tiene que hacer la tarea.
        public required Guid AssignedToId { get; set; }

        // Reporte que originó la tarea (opcional).
        public Guid? ReportId { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
