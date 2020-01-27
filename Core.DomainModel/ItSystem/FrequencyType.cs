using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown type for the <see cref="DataRowUsage"/>. Represents the frequency of
    /// the usage of the DataRow.
    /// </summary>
    public class FrequencyType : OptionEntity<DataRowUsage>, IOptionReference<DataRowUsage>
    {
        public virtual ICollection<DataRowUsage> References { get; set; } = new HashSet<DataRowUsage>();

        public virtual ICollection<SystemRelation> SystemRelations { get; set; } = new HashSet<SystemRelation>();
    }
}
