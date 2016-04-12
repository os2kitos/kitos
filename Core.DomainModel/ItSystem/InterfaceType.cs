using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown type for the <see cref="ItSystem"/>.
    /// Provides details about an ItSystem of type interface.
    /// </summary>
    public class InterfaceType : Entity, IOptionEntity<ItInterface>
    {
        public InterfaceType()
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
