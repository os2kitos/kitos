using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown option for ItSystem, representing the business type of the system
    /// </summary>
    public class BusinessType : Entity, IOptionEntity<ItSystem>
    {
        public BusinessType()
        {
            References = new List<ItSystem>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }

        /// <summary>
        /// The ItSystems that has been marked with this BusinessType 
        /// </summary>
        public virtual ICollection<ItSystem> References { get; set; }
    }
}