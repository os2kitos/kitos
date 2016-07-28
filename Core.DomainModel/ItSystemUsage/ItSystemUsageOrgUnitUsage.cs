using Core.DomainModel.Organization;

namespace Core.DomainModel.ItSystemUsage
{
    public class ItSystemUsageOrgUnitUsage
    {
        public int ItSystemUsageId { get; set; }
        public virtual ItSystemUsage ItSystemUsage { get; set; }

        public int OrganizationUnitId { get; set; }
        public virtual OrganizationUnit OrganizationUnit { get; set; }

        public virtual ItSystemUsage ResponsibleItSystemUsage { get; set; }
    }
}
