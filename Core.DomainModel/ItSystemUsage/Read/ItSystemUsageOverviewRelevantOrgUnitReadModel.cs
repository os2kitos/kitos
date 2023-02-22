using System;

namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewRelevantOrgUnitReadModel : IHasId
    {
        public int Id { get; set; }
        public Guid OrganizationUnitUuid { get; set; }
        public int OrganizationUnitId { get; set; }
        public string OrganizationUnitName { get; set; }
        public int ParentId { get; set; }
        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
