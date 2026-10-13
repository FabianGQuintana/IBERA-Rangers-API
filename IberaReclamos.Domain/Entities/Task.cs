using System;
using System.Collections.Generic;
using System.Text;
using IberaReclamos.Domain.Enum;

namespace IberaReclamos.Domain.Entities
{
    public class Task : BaseEntity
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        public required string AssignedBy { get; set; }

        public required string AssignedTo { get; set; }

        public required TypeStatusReportAndTasks StatusTask { get; set; }

        //Fk
        public required Report ReportId { get; set; } 
    }
}
