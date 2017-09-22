using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public class HandoverTrialType : OptionEntity<HandoverTrial>, IOptionReference<HandoverTrial>
    {
        public virtual ICollection<HandoverTrial> References { get; set; } = new HashSet<HandoverTrial>();
    }
}
