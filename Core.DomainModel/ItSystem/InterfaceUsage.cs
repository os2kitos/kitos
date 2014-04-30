using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystem
{
    public class InterfaceUsage : IEntity<int>
    {
        public InterfaceUsage()
        {
            this.DataRowUsages = new List<DataRowUsage>();
        }

        public int Id { get; set; }

        public int ItSystemUsageId { get; set; }
        public virtual ItSystemUsage ItSystemUsage { get; set; }

        public int InterfaceId { get; set; }
        public virtual ItSystem Interface { get; set; }

        public virtual ICollection<DataRowUsage> DataRowUsages { get; set; }

        public int? InfrastructureId { get; set; }
        public virtual ItSystem Infrastructure { get; set; }

        public int? InterfaceCategoryId { get; set; }
        public InterfaceCategory InterfaceCategory { get; set; }
    }
}
