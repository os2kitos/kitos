namespace Core.DomainModel
{
    public class KendoOrganizationalConfiguration : Entity, IOwnedByOrganization
    {
        public OverviewType OverviewType { get; set; }
        public string Configuration { get; set; }
        public int OrganizationId { get; set; }
        public Organization.Organization Organization { get; set; }
    }

    public enum OverviewType
    {
        ItSystemUsage = 0
    }
}
