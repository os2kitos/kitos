namespace Core.DomainModel
{
    public class OrganizationRight : IRight<OrganizationUnit, OrganizationRole>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public virtual User User { get; set; }
        public virtual OrganizationRole Role { get; set; }
        public virtual OrganizationUnit Object { get; set; }
    }
}