using System.Collections.Generic;

namespace Core.DomainModel
{
    /// <summary>
    /// Administrator roles, suchs a Local administrator.
    /// </summary>
    /// <remarks>
    /// Notice that global administrator is not an OrganizationRight
    /// because it's not connected to any organization
    /// </remarks>
    public class OrganizationRole : Entity, IRoleEntity<OrganizationRight>
    {
        public OrganizationRole()
        {
            References = new List<OrganizationRight>();
            HasReadAccess = true;
            HasWriteAccess = true;
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<OrganizationRight> References { get; set; }
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
    }
}
