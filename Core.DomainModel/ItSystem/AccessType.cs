using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class AccessType : Entity, ISystemModule
    {
        public AccessType()
        {
            this.ItSystemUsages = new List<ItSystemUsage.ItSystemUsage>();
        }

        public string Name { get; set; }
        public int ItSystemId { get; set; }
        public virtual ItSystem ItSystem { get; set; }
        public virtual ICollection<ItSystemUsage.ItSystemUsage> ItSystemUsages { get; set; }
    }
}
