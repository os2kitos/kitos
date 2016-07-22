using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project role.
    /// </summary>
    public class ItProjectRole : Entity, IRoleEntity, IOptionEntity<ItProjectRight>
    {
        public ItProjectRole()
        {
            HasReadAccess = true;
            References = new List<ItProjectRight>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItProjectRight> References { get; set; }
    }
}
