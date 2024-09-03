namespace Core.DomainModel.GDPR.Read
{
    public class DataProcessingRegistrationRoleAssignmentReadModel : IHasId
    {
        public string Email { get; set; }

        public int Id { get; set; }
        
        public int RoleId { get; set; }
     
        public int UserId { get; set; }
        
        public string UserFullName { get; set; }

        public int ParentId { get; set; }

        public virtual DataProcessingRegistrationReadModel Parent { get; set; }
    }
}
