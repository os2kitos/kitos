using System.Collections.Generic;
using Core.DomainModel.Advice;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract role option.
    /// </summary>
    public class ItContractRole : OptionEntity<ItContractRight>, IRoleEntity, IOptionReference<ItContractRight>
    {
        public ItContractRole()
        {
            HasReadAccess = true;
        }
        
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
        public virtual ICollection<ItContractRight> References { get; set; } = new HashSet<ItContractRight>();
        public virtual ICollection<AdviceUserRelation> AdviceUserRelations { get; set; } = new List<AdviceUserRelation>();
    }
}
