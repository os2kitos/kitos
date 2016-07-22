﻿namespace Core.DomainModel
{
    /// <summary>
    /// Represents that a user has an administrator role on an organization.
    /// </summary>
    public class OrganizationRight : Entity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public OrganizationRole Role { get; set; }
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
        public int? DefaultOrgUnitId { get; set; }
        public virtual OrganizationUnit DefaultOrgUnit { get; set; }
    }
}
