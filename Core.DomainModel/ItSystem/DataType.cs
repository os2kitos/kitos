using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown type for the <see cref="DataRow"/>. Represents the type of
    /// data that is exposed by the DataRow.
    /// </summary>
    public class DataType : Entity, IOptionEntity<DataRow>
    {
        public DataType()
        {
            References = new List<DataRow>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }

        /// <summary>
        /// The DataRows that is using this data type
        /// </summary>
        public virtual ICollection<DataRow> References { get; set; }
    }
}
