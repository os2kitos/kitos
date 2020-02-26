namespace Core.DomainModel.Reports
{
    public class Report : Entity, IReportModule, IContextAware, IHasAccessModifier, IOwnedByOrganization, IHasName
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int? CategoryTypeId { get; set; }
        public virtual ReportCategoryType CategoryType { get; set; }
        public int OrganizationId { get; set; }
        public virtual Organization.Organization Organization { get; set; }

        /// <summary>
        /// report definition saved as a json string
        /// </summary>
        public string Definition { get; set; }

        public bool IsInContext(int organizationId)
        {
            return organizationId == OrganizationId;
        }

        public AccessModifier AccessModifier { get; set; }
    }
}