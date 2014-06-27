using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represent the local usage of a interface.
    /// When an <see cref="ItSystem"/>, which can use an interface, is taken into local usage,
    /// a InterfaceUsage is created, to allow for adding local details regarding the usage 
    /// of the interface.
    /// It is also used for binding an <see cref="ItContract"/> with the usage.
    /// </summary>
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

        public int? ItContractId { get; set; }
        /// <summary>
        /// The contract for this interface usage.
        /// </summary>
        public virtual ItContract.ItContract ItContract { get; set; }

        public int InterfaceId { get; set; }
        /// <summary>
        /// The interface that is being used.
        /// </summary>
        public virtual ItSystem Interface { get; set; }

        /// <summary>
        /// Local details regarding the usage of the exposed data of the interface
        /// </summary>
        public virtual ICollection<DataRowUsage> DataRowUsages { get; set; }

        public int? InfrastructureId { get; set; }
        /// <summary>
        /// An ItSystem marked as infrastructure for the local usage of the interface.
        /// </summary>
        public virtual ItSystem Infrastructure { get; set; }
        
        /// <summary>
        /// Whether local usage of the interface is wanted or not.
        /// </summary>
        public bool IsWishedFor { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (ItSystemUsage != null && ItSystemUsage.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
