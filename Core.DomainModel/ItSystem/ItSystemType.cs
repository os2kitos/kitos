using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class ItSystemType : OptionEntity<ItSystem>, IOptionReference<ItSystem>
    {
        public virtual ICollection<ItSystem> References { get; set; } = new HashSet<ItSystem>();
    }
}
