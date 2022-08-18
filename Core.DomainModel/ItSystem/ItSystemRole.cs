using System.Collections.Generic;
using Core.DomainModel.Advice;

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
        public virtual ICollection<AdviceUserRelation> AdviceUserRelations { get; set; }
    }
}
