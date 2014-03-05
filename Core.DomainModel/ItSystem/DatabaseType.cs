using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class DatabaseType : IOptionEntity<Technology>
    {
        public DatabaseType()
        {
            References = new List<Technology>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<Technology> References { get; set; }
    }
}
