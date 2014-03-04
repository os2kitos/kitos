using System.Collections.Generic;

namespace Core.DomainModel
{
    public class DropDownEntity<T> : IEntity<int>
    {
        public DropDownEntity()
        {
            this.References = new List<T>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }

        public virtual ICollection<T> References { get; set; }
    }
}