using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Dropdown type for the <see cref="SystemRelation"/>. Represents the frequency of
    /// the usage of the DataRow.
    /// </summary>
    public class RelationFrequencyType : OptionEntity<SystemRelation>, IOptionReference<SystemRelation>
    {
        public virtual ICollection<SystemRelation> References { get; set; } = new HashSet<SystemRelation>();
    }
}
