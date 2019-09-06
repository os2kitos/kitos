using System.Collections.Generic;

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
    }
}
