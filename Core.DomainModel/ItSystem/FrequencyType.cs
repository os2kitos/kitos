using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown type for the <see cref="DataRowUsage"/>. Represents the frequency of
    /// the usage of the DataRow.
    /// </summary>
    public class FrequencyType : Entity, IOptionEntity<DataRowUsage>
    {
        public FrequencyType()
        {
            this.References = new List<DataRowUsage>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }

        /// <summary>
        /// The DataRowUsages that uses this frequency dropdown.
        /// </summary>
        public virtual ICollection<DataRowUsage> References { get; set; }
    }
}
