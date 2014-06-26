using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown type for the <see cref="ItSystem"/>.  
    /// Provides details about an ItSystem of type interface.
    /// </summary>
    /// <remarks>
    /// Notice that this is NOT an interface, nor does it 
    /// distinguish systems from interfaces.</remarks>
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
