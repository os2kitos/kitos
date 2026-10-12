using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class SystemUsageCriticalityLevel : OptionEntity<ItSystemUsage.ItSystemUsage>, IOptionReference<ItSystemUsage.ItSystemUsage>
    {
        public virtual ICollection<ItSystemUsage.ItSystemUsage> References { get; set; } = new HashSet<ItSystemUsage.ItSystemUsage>();
    }
}
