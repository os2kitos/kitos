using System.Collections.Generic;

namespace Core.DomainModel
{
    // TODO DELETE THIS? Currently local admin cannot change names of modules
    /// <summary>
    /// Local configuration of the name of the ItSystem module.
    /// </summary>
    public class ItSystemModuleName : Entity, IOptionEntity<Config>
    {
        public ItSystemModuleName()
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