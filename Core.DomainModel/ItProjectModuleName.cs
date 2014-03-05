using System.Collections.Generic;

namespace Core.DomainModel
{
    public class ItProjectModuleName : IOptionEntity<Config>
    {
        public ItProjectModuleName()
        {
            this.IsActive = true;
            this.References = new List<Config>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }

        public virtual ICollection<Config> References { get; set; }
    }
}