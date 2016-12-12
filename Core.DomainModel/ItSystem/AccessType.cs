using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystem
{
    public class AccessType : Entity
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
