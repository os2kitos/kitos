namespace Core.DomainModel.ItContract.Read
{
    public class ItContractOverviewRoleAssignmentReadModel : IHasId
    {
        public int Id { get; set; }

        public int RoleId { get; set; }

        public int UserId { get; set; }

        public string UserFullName { get; set; }

        public string Email { get; set; }

        public int ParentId { get; set; }

        public virtual ItContractOverviewReadModel Parent { get; set; }
    }
}
