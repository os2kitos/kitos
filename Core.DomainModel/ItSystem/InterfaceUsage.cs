using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystem
{
    public class InterfaceUsage : Entity
    {
        public InterfaceUsage()
        {
            this.DataRowUsages = new List<DataRowUsage>();
        }
        
        public int ItSystemUsageId { get; set; }
        /// <summary>
        /// The system that is using the interface.
        /// </summary>
        public virtual ItSystemUsage ItSystemUsage { get; set; }

        /// <summary>
        /// The contract for this interface usage.
        /// </summary>
        public int? ItContractId { get; set; }
        public virtual ItContract.ItContract ItContract { get; set; }

        public int InterfaceId { get; set; }
        /// <summary>
        /// The interface that is being used.
        /// </summary>
        public virtual ItSystem Interface { get; set; }

        public virtual ICollection<DataRowUsage> DataRowUsages { get; set; }

        public int? InfrastructureId { get; set; }
        public virtual ItSystem Infrastructure { get; set; }

        public int? InterfaceCategoryId { get; set; }
        public InterfaceCategory InterfaceCategory { get; set; }

        /// <summary>
        /// Whether local usage of the interface is wanted or not.
        /// </summary>
        public bool IsWishedFor { get; set; }
    }
}
