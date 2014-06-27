using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown type for the <see cref="DataRowUsage"/>. Represents the frequency of 
    /// the usage of the DataRow.
    /// </summary>
    public class Frequency : Entity, IOptionEntity<DataRowUsage>
    {
        public Frequency()
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
