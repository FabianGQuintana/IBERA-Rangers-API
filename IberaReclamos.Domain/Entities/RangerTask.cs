using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Domain.Entities
{
    // Tarea asignada a un ranger (ej: "Limpiar el punto del reporte X").
    // Se llama RangerTask y no Task para no chocar con System.Threading.Tasks.Task.
    public class RangerTask : BaseEntity
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        public required TypeStatusReportAndTasks StatusTask { get; set; }

        public DateTime? DueDate { get; set; }

        //Fks
        public Guid AssignedById { get; set; }

        public Guid AssignedToId { get; set; }

        // Opcional: una tarea puede nacer de un reporte o ser independiente.
        public Guid? ReportId { get; set; }

        //Navegation
        public virtual User AssignedBy { get; set; } = null!;

        public virtual User AssignedTo { get; set; } = null!;

        public virtual Report? Report { get; set; }
    }
}
