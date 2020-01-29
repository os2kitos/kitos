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

        public SystemRelation(ItSystemUsage relationSource, ItSystemUsage relationTarget)
        {
            RelationSource = relationSource ?? throw new ArgumentNullException(nameof(relationSource));
            RelationTarget = relationTarget ?? throw new ArgumentNullException(nameof(relationTarget));
            Reference = new ExternalLink();
        }

        /// <summary>
        /// Mandatory relation target
        /// </summary>
        public int RelationSourceId { get; set; }
        /// <summary>
        /// Mandatory relation target
        /// </summary>
        public virtual ItSystemUsage RelationSource { get; set; }
        /// <summary>
        /// Mandatory relation target
        /// </summary>
        public int RelationTargetId { get; set; }
        /// <summary>
        /// Mandatory relation target
        /// </summary>
        public virtual ItSystemUsage RelationTarget { get; set; }
        /// <summary>
        /// Defines the optional interface to the <see cref="RelationTarget"/>
        /// </summary>
        public virtual ItInterface RelationInterface { get; set; }
        /// <summary>
        /// Optional description for the relation
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Link to information about the relation
        /// </summary>
        public virtual ExternalLink Reference { get; set; }
        /// <summary>
        /// Optional usage frequency
        /// </summary>
        public virtual RelationFrequencyType UsageFrequency { get; set; }
        /// <summary>
        /// Optional contract relation
        /// </summary>
        public virtual ItContract.ItContract AssociatedContract { get; set; }
    }
}
