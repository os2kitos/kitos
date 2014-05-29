using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class Interface : Entity, IOptionEntity<ItSystem>
    {
        public Interface()
        {
            References = new List<ItSystem>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItSystem> References { get; set; }
    }
}
