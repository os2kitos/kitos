namespace Core.DomainModel
{
    /// <summary>
    /// Represents that a user has an administrator role on an organization.
    /// </summary>
    public class AdminRight : Entity, IRight<Organization, AdminRight, AdminRole>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public virtual User User { get; set; }
        public virtual AdminRole Role { get; set; }
        public virtual Organization Object { get; set; }
        public int? DefaultOrgUnitId { get; set; }
        public virtual Organization DefaultOrgUnit { get; set; }
    }
}
