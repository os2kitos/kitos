using System;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel.ItSystemUsage
{
    public class SystemRelation : Entity
    {
        /// <summary>
        /// NOTE: For EF
        /// </summary>
        private SystemRelation() { }

        public SystemRelation(ItSystemUsage relationTarget)
        {
            RelationTarget = relationTarget ?? throw new ArgumentNullException(nameof(relationTarget));
        }

        /// <summary>
        /// Mandatory relation target
        /// </summary>
        public ItSystemUsage RelationTarget { get; set; }
        /// <summary>
        /// Defines the optional interface to the <see cref="RelationTarget"/>
        /// </summary>
        public ItInterface RelationInterface { get; set; }
        /// <summary>
        /// Optional description for the relation
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Optional usage frequency
        /// </summary>
        public FrequencyType UsageFrequency { get; set; }
        /// <summary>
        /// Optional contract relation
        /// </summary>
        public ItContract.ItContract AssociatedContract { get; set; }
    }
}
