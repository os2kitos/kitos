namespace Core.DomainModel
{
    public class KendoOrganizationalConfiguration : Entity, IOwnedByOrganization
    {

        public OverviewType OverviewType { get; set; }
        public int Version { get; set; }
        public string VisibleColumnsCsv { get; set; }
        public int OrganizationId { get; set; }
        public Organization.Organization Organization { get; set; }

        public void UpdateVersion()
        {
            Version = VisibleColumnsCsv.GetHashCode();
        }
    }

    public enum OverviewType
    {
        ItSystemUsage = 0
    }
}
