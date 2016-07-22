using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// It system role option.
    /// </summary>
    public class ItSystemRole : Entity, IRoleEntity, IOptionEntity<ItSystemRight>
    {
        public ItSystemRole()
        {
            HasReadAccess = true;
            References = new List<ItSystemRight>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItSystemRight> References { get; set; }
    }
}
