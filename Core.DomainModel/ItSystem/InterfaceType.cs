using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown type for the <see cref="ItInterface"/>.
    /// Provides details about an ItSystem of type interface.
    /// </summary>
    /// <remarks>
    /// Notice that this is NOT an interface, nor does it
    /// distinguish systems from interfaces.
    /// </remarks>
    public class InterfaceType : Entity, IOptionEntity<ItInterface> // TODO find a better suited name for this, note that InterfaceType is taken
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
