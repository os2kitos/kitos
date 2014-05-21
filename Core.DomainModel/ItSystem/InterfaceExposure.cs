namespace Core.DomainModel.ItSystem
{
    public class InterfaceExposure : IEntity<int>
    {

        public int Id { get; set; }

        public int ItSystemUsageId { get; set; }
        /// <summary>
        /// The system that is exposing the interface.
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
        public bool WishedFor { get; set; }
    }
}