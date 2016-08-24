using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class ItSystemType : Entity, IOptionEntity<ItSystem>
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public ICollection<ItSystem> References { get; set; }
    }
}
