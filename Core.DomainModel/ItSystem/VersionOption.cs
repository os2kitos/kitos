using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class VersionOption : Entity, IOptionEntity<ItInterface>
    {
        public VersionOption()
        {
            References = new List<ItInterface>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItInterface> References { get; set; }
    }
}
