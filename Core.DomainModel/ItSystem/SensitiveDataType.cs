using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class SensitiveDataType : Entity, IOptionEntity<ItSystemUsage.ItSystemUsage>
    {
        public SensitiveDataType()
        {
            this.References = new List<ItSystemUsage.ItSystemUsage>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItSystemUsage.ItSystemUsage> References { get; set; }
    }
}