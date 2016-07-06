using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public class HandoverTrialType : Entity, IOptionEntity<HandoverTrial>
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public ICollection<HandoverTrial> References { get; set; }
    }
}
