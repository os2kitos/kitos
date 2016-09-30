using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown type for the <see cref="DataRow"/>. Represents the type of
    /// data that is exposed by the DataRow.
    /// </summary>
    public class DataType : OptionEntity<DataRow>, IOptionReference<DataRow>
    {
        public virtual ICollection<DataRow> References { get; set; } = new HashSet<DataRow>();
    }
}
