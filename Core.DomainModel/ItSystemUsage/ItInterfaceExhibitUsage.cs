using Core.DomainModel.ItSystem;

namespace Core.DomainModel.ItSystemUsage
{
    /// <summary>
    /// Represent the local usage of an exhibited interface.
    /// When an <see cref="ItSystem"/>, which exhibits an interface, is taken into local usage,
    /// a ItInterfaceExhibitUsage is created, to allow for adding local details regarding the exposure
    /// of the interface.
    /// It is also used for binding an <see cref="ItContract"/> with the usage.
    /// </summary>
    public class ItInterfaceExhibitUsage : ISystemModule
    {
        public object[] GetKey()
        {
            return new object[]{ItSystemUsageId, ItInterfaceExhibitId};
        }
        public int ItSystemUsageId { get; set; }
        /// <summary>
        /// The local usage of the system that is exposing the interface.
        /// </summary>
        public virtual ItSystemUsage ItSystemUsage { get; set; }

        /// <summary>
        /// The contract for this interface exposure.
        /// </summary>
        public int? ItContractId { get; set; }
        public virtual ItContract.ItContract ItContract { get; set; }

        public int ItInterfaceExhibitId { get; set; }
        /// <summary>
        /// The interface that is being exhibited.
        /// </summary>
        public virtual ItInterfaceExhibit ItInterfaceExhibit { get; set; }

        /// <summary>
        /// Whether local exposure of the interface is wanted or not.
        /// </summary>
        public bool IsWishedFor { get; set; }
    }
}
