namespace Core.DomainModel.ItSystemUsage.Read
{
    public class ItSystemUsageOverviewRoleAssignmentReadModel : IHasId
    {
        public int Id { get; set; }

        public int RoleId { get; set; }

        public int UserId { get; set; }

        public string UserFullName { get; set; }

        public int ParentId { get; set; }

        public virtual ItSystemUsageOverviewReadModel Parent { get; set; }
    }
}
