namespace Core.DomainModel.Reports
{
    public class Report : Entity
    {
        public Report()
        {
            Access = ReportAccess.Private;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public ReportCategory Category { get; set; }
        public ReportAccess Access { get; set; }
        /// <summary>
        /// report definition saved as a json string
        /// </summary>
        public string Definition { get; set; }
    }
}