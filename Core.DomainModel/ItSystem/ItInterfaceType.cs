using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown type for the <see cref="ItSystem"/>.
    /// Provides details about an ItSystem of type interface.
    /// </summary>
    public class ItInterfaceType : OptionEntity<ItInterface>, IOptionReference<ItInterface>
    {
        public virtual ICollection<ItInterface> References { get; set; } = new HashSet<ItInterface>();
    }
}
