namespace Core.DomainModel
{
    /// <summary>
    /// Represents that a user has an administrator role on an organization.
    /// </summary>
    public class OrganizationRight : Entity, IRight<Organization, OrganizationRight, OrganizationRole>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public virtual User User { get; set; }
        public virtual OrganizationRole Role { get; set; }
        public virtual Organization Object { get; set; }
        public int? DefaultOrgUnitId { get; set; }
        public virtual OrganizationUnit DefaultOrgUnit { get; set; }
    }
}
