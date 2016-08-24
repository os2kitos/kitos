using System.Collections.Generic;

namespace Core.DomainModel
{
    /// <summary>
    /// Represents a role that a user can have on an organization unit,
    /// such as an employee or boss.
    /// </summary>
    public class OrganizationUnitRole : Entity, IRoleEntity<OrganizationUnitRight>
    {
        public OrganizationUnitRole()
        {
            HasReadAccess = true;
            References = new List<OrganizationUnitRight>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
        public string Note { get; set; }
        public virtual ICollection<OrganizationUnitRight> References { get; set; }
    }
}
