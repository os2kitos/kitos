using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class Environment : IDropDownEntity<Technology>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<Technology> References { get; set; }
    }
}
