// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Defines the ItSystemCategories type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    using Core.DomainModel.ItSystemUsage;

    public class ItSystemCategories : OptionEntity<ItSystemUsage>, IOptionReference<ItSystemUsage>
    {
        public virtual ICollection<ItSystemUsage> References { get; set; } = new HashSet<ItSystemUsage>();
    }
}
