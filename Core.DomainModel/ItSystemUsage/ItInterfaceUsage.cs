using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel.ItSystemUsage
{
    /// <summary>
    /// Represent the local usage of a interface.
    /// When an <see cref="ItSystem"/>, which can use an interface, is taken into local usage,
    /// a ItInterfaceUsage is created, to allow for adding local details regarding the usage
    /// of the interface.
    /// It is also used for binding an <see cref="ItContract"/> with the usage.
    /// </summary>
    public class ItInterfaceUsage : ISystemModule, IContractModule
    {

        public static object[] GetKey(int usageId, int systemId, int interfaceId)
        {
            return new object[] {usageId, systemId, interfaceId};
        }

        public ItInterfaceUsage()
        {
            this.DataRowUsages = new List<DataRowUsage>();
        }

        public int ItSystemUsageId { get; set; }
        /// <summary>
        /// The system that is using the interface.
        /// </summary>
        public virtual ItSystemUsage ItSystemUsage { get; set; }

        public int? ItContractId { get; set; }
        /// <summary>
        /// The contract for this interface usage.
        /// </summary>
        public virtual ItContract.ItContract ItContract { get; set; }

        /// <summary>
        /// Local details regarding the usage of the exposed data of the interface
        /// </summary>
        public virtual ICollection<DataRowUsage> DataRowUsages { get; set; }

        public int? InfrastructureId { get; set; }
        /// <summary>
        /// An ItSystem marked as infrastructure for the local usage of the interface.
        /// </summary>
        public virtual ItSystemUsage Infrastructure { get; set; }

        public int ItInterfaceId { get; set; }
        public int ItSystemId { get; set; }
        public virtual ItInterface ItInterface { get; set; }

        /// <summary>
        /// Whether local usage of the interface is wanted or not.
        /// </summary>
        public bool IsWishedFor { get; set; }
    }
}
