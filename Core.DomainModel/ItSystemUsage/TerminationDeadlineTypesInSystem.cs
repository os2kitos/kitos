using System.Collections.Generic;

namespace Core.DomainModel.ItSystemUsage
{
    public class TerminationDeadlineTypesInSystem : OptionEntity<ItSystem.ItSystem>, IOptionReference<ItSystem.ItSystem>
    {
        public virtual ICollection<ItSystem.ItSystem> References { get; set; } = new HashSet<ItSystem.ItSystem>();
    }

}
