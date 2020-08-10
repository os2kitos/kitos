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
    public class InterfaceType : OptionEntity<ItInterface>, IOptionReference<ItInterface> 
    {
        public virtual ICollection<ItInterface> References { get; set; } = new HashSet<ItInterface>();
    }
}
