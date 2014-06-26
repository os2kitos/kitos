using System.Collections.Generic;

namespace Core.DomainModel
{
    /// <summary>
    /// Represents a role that a user can have on an organization unit,
    /// such as an employee or boss.
    /// </summary>
    public class OrganizationRole : Entity, IRoleEntity<OrganizationRight>
    {
        public OrganizationRole()
        {
            HasReadAccess = true;
            References = new List<OrganizationRight>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
        public string Note { get; set; }
        public virtual ICollection<OrganizationRight> References { get; set; }
    }
}
