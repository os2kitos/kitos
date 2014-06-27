using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown type for the <see cref="ItSystem"/>.  
    /// Provides details about an ItSystem of type interface.
    /// </summary>
    public class InterfaceType : Entity, IOptionEntity<ItSystem>
    {
        public InterfaceType()
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