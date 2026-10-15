using IberaReclamos.Domain.Enum;


namespace IberaReclamos.Domain.Entities
{
    public class ReportCategory : BaseEntity
    {
        public required SeverityLevelReport SeverityLevel { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        //Navegacion inversa
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
