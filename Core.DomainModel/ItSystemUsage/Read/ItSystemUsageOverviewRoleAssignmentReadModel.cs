using System;

namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewRoleAssignmentReadModel : IHasId
    {
        public int Id { get; set; }

        public Guid RoleUuid { get; set; }
        public int RoleId { get; set; }

        public int UserId { get; set; }

        public string UserFullName { get; set; }

        public string Email { get; set; }

        public int ParentId { get; set; }

        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
