using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown option for ItSystem, representing the business type of the system
    /// </summary>
    public class BusinessType : OptionEntity<ItSystem>, IOptionReference<ItSystem>, IHasUuid
    {
        public virtual ICollection<ItSystem> References { get; set; } = new HashSet<ItSystem>();
    }
}