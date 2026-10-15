using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Domain.Entities
{
    // Incidente ambiental reportado por un guardaparque en el terreno (lixiviados, plásticos, etc.).
    public class Report : BaseEntity
    {
        //Properties
        // Coordenadas GPS en grados decimales (double para poder filtrar/calcular distancias).
        public required double Latitude { get; set; }

        public required double Longitude { get; set; }

        public string? Notes { get; set; }

        // Momento en que el ranger tomó la foto/reporte en el terreno (puede ser offline).
        public required DateTime CaptureDate { get; set; }

        // Momento en que el reporte llegó al servidor (sincronización).
        public DateTime? SyncDate { get; set; }

        public required TypeStatusReportAndTasks StatusReport { get; set; }

        // Id generado en el dispositivo cuando el ranger está offline.
        // Permite que la sincronización (bulk insert) sea idempotente: si llega dos veces, no se duplica.
        public Guid? ClientId { get; set; }

        // Foto en Cloudflare R2: guardamos la clave del objeto y la URL pública, nunca el archivo en la DB.
        public string? PhotoKey { get; set; }

        public string? PhotoUrl { get; set; }

        //FKs
        public Guid ReportCategoryId { get; set; }

        public Guid ReportedById { get; set; }

        //Navegation
        public virtual ReportCategory Category { get; set; } = null!;

        public virtual User ReportedBy { get; set; } = null!;

        //Navegacion inversa
        public virtual ICollection<RangerTask> Tasks { get; set; } = new List<RangerTask>();
    }
}
