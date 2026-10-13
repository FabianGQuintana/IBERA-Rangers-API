using System;
using System.Collections.Generic;
using System.Text;
using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Domain.Entities
{
    public class Report : BaseEntity 
    {
        //Properties
        public required string Longitude {  get; set; }

        public required string Latitude { get; set; }    

        public  string? Notes { get; set; }

        public  DateTime? SyncDate { get; set; }

        public required DateTime CaptureDate { get; set; }

        public required TypeStatusReportAndTasks StatusReport {  get; set; }

        //FKs
        public required ReportCategory IdReportCategory { get; set; }

        //Navegation
        public required virtual ReportCategory Category { get; set; }

        //Navegacion inversa
        public virtual ICollection<Task> Tags { get; set; } = new List<Task>();
    }
}
