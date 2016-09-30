using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class SensitiveDataType : OptionEntity<ItSystemUsage.ItSystemUsage>, IOptionReference<ItSystemUsage.ItSystemUsage>
    {
        public virtual ICollection<ItSystemUsage.ItSystemUsage> References { get; set; } = new HashSet<ItSystemUsage.ItSystemUsage>();
    }
}
