using System;
using System.Collections.Generic;

namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewReadModel : IOwnedByOrganization, IReadModel<ItSystemUsage>, IHasName
    {
        public ItSystemUsageOverviewReadModel()
        {
            RoleAssignments = new List<ItSystemUsageOverviewRoleAssignmentReadModel>();
        }

        public int OrganizationId { get; set; }
        public Organization.Organization Organization { get; set; }
        public int Id { get; set; }
        public int SourceEntityId { get; set; }
        public ItSystemUsage SourceEntity { get; set; }
        public string Name { get; set; }
        public bool ItSystemDisabled { get; set; }
        public bool IsActive { get; set; }
        public string ParentItSystemName { get; set; }
        public int? ParentItSystemId { get; set; }
        public string Version { get; set; }
        public string LocalCallName { get; set; }
        public string LocalSystemId { get; set; }
        public Guid ItSystemUuid { get; set; }
        public virtual ICollection<ItSystemUsageOverviewRoleAssignmentReadModel> RoleAssignments { get; set; }
    }
}
