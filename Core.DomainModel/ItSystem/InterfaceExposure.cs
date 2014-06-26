namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represent the local exposure of a interface.
    /// When an <see cref="ItSystem"/>, which exposes an interface, is taken into local usage,
    /// a InterfaceExposure is created, to allow for adding local details regarding the exposure 
    /// of the interface. 
    /// It is also used for binding an <see cref="ItContract"/> with the usage.
    /// </summary>
    public class InterfaceExposure : Entity
    {
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

        public int InterfaceId { get; set; }
        /// <summary>
        /// The interface that is being exposed.
        /// </summary>
        public virtual ItSystem Interface { get; set; }

        /// <summary>
        /// Whether local exposure of the interface is wanted or not.
        /// </summary>
        public bool IsWishedFor { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (ItSystemUsage != null && ItSystemUsage.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}