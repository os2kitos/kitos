using System.Collections.Generic;
using Core.DomainModel.Advice;

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
        public virtual ICollection<AdviceUserRelation> AdviceUserRelations { get; set; } = new List<AdviceUserRelation>();
    }
}
