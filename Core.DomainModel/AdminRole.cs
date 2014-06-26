using System.Collections.Generic;

namespace Core.DomainModel
{
    /// <summary>
    /// Administrator roles, suchs a Local administrator.
    /// </summary>
    /// <remarks>
    /// Notice that global administrator is not an AdminRight
    /// because it's not connected to any organization
    /// </remarks>
    public class AdminRole : Entity, IRoleEntity<AdminRight>
    {
        public AdminRole()
        {
            References = new List<AdminRight>();
            HasReadAccess = true;
            HasWriteAccess = true;
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<AdminRight> References { get; set; }
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
    }
}