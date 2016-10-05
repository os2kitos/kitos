using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project role.
    /// </summary>
    public class ItProjectRole : OptionEntity<ItProjectRight>, IRoleEntity, IOptionReference<ItProjectRight>
    {
        public ItProjectRole()
        {
            HasReadAccess = true;
        }
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
        public virtual ICollection<ItProjectRight> References { get; set; } = new HashSet<ItProjectRight>();
    }
}
