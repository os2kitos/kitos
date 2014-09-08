namespace Core.DomainModel.ItSystem
{
    public class ItSystemUsageOrgUnitUsage
    {
        public int ItSystemUsageId { get; set; }
        public virtual ItSystemUsage ItSystemUsage { get; set; }

        public int OrganizationUnitId { get; set; }
        public virtual OrganizationUnit OrganizationUnit { get; set; }
    }
}
