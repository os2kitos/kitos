using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// It system role option.
    /// </summary>
    public class ItSystemRole : OptionEntity<ItSystemRight>, IRoleEntity, IOptionReference<ItSystemRight>
    {
        public bool HasReadAccess { get; set; }

        public bool HasWriteAccess { get; set; }
        public virtual ICollection<ItSystemRight> References { get; set; } = new HashSet<ItSystemRight>();
    }
}
