using System.Collections.Generic;

namespace Core.DomainModel
{
    // TODO DELETE THIS?
    public class ItContractModuleName : Entity, IOptionEntity<Config>
    {
        public ItContractModuleName()
        {
            this.IsActive = true;
            this.References = new List<Config>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }

        public virtual ICollection<Config> References { get; set; }
    }
}