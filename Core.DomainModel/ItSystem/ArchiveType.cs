using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Option for archive pane
    /// </summary>
    public class ArchiveTestLocation : OptionEntity<ItSystemUsage.ItSystemUsage>, IOptionReference<ItSystemUsage.ItSystemUsage>
    {
        public virtual ICollection<ItSystemUsage.ItSystemUsage> References { get; set; } = new HashSet<ItSystemUsage.ItSystemUsage>();
    }
}
