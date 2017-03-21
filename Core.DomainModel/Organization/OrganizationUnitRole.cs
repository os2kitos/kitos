using System.Collections.Generic;
// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.Organization
{
    /// <summary>
    /// Represents a role that a user can have on an organization unit,
    /// such as an employee or boss.
    /// </summary>
    public class OrganizationUnitRole : OptionEntity<OrganizationUnitRight>, IRoleEntity, IOptionReference<OrganizationUnitRight>, IOrganizationModule
    {
        public bool HasReadAccess { get; set; }

        public bool HasWriteAccess { get; set; }

        public virtual ICollection<OrganizationUnitRight> References { get; set; } = new HashSet<OrganizationUnitRight>();
    }
}
