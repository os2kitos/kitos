namespace Core.DomainModel.Organization
{
    /// <summary>
    /// Represents a users rights on an Organization Unit
    /// </summary>
    public class OrganizationUnitRight : Entity, IRight<OrganizationUnit, OrganizationUnitRight, OrganizationUnitRole>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public virtual User User { get; set; }
        public virtual OrganizationUnitRole Role { get; set; }
        public virtual OrganizationUnit Object { get; set; }
    }
}
