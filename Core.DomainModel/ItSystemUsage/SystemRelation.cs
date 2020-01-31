using System;
using Core.DomainModel.Extensions;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;

namespace Core.DomainModel.ItSystemUsage
{
    public class SystemRelation : Entity
    {
        //NOTE: For EF
        protected SystemRelation() { }

        public SystemRelation(ItSystemUsage fromSystemUsage)
        {
            FromSystemUsage = fromSystemUsage ?? throw new ArgumentNullException(nameof(fromSystemUsage));
        }

        /// <summary>
        /// Mandatory relation "from"
        /// </summary>
        public int FromSystemUsageId { get; set; }
        /// <summary>
        /// Mandatory relation "from"
        /// </summary>
        public virtual ItSystemUsage FromSystemUsage { get; set; }
        /// <summary>
        /// Mandatory relation "to"
        /// </summary>
        public int ToSystemUsageId { get; set; }
        /// <summary>
        /// Mandatory relation "to"
        /// </summary>
        public virtual ItSystemUsage ToSystemUsage { get; set; }
        /// <summary>
        /// Defines the optional interface to the <see cref="ToSystemUsage"/>
        /// </summary>
        public int? RelationInterfaceId { get; set; }
        /// <summary>
        /// Defines the optional interface to the <see cref="ToSystemUsage"/>
        /// </summary>
        public virtual ItInterface RelationInterface { get; set; }
        /// <summary>
        /// Optional description for the relation
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Link to information about the relation
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// Optional usage frequency
        /// </summary>
        public int? UsageFrequencyId { get; set; }
        /// <summary>
        /// Optional usage frequency
        /// </summary>
        public virtual RelationFrequencyType UsageFrequency { get; set; }
        /// <summary>
        /// Optional contract relation
        /// </summary>
        public int? AssociatedContractId { get; set; }
        /// <summary>
        /// Optional contract relation
        /// </summary>
        public virtual ItContract.ItContract AssociatedContract { get; set; }
        /// <summary>
        /// Replaces mandatory relation "to" and resets relation interface
        /// </summary>
        /// <param name="toSystemUsage">Replacement system usage</param>
        public Result<ItSystemUsage, OperationError> SetRelationTo(ItSystemUsage toSystemUsage)
        {
            if(toSystemUsage == null)
                throw new ArgumentNullException(nameof(toSystemUsage));

            
            if (FromSystemUsage.Id == toSystemUsage.Id)
            {
                return new OperationError("'From' cannot equal 'To'", OperationFailure.BadInput);
            }

            if (!FromSystemUsage.IsInSameOrganizationAs(toSystemUsage))
            {
                return new OperationError("Attempt to create relation to it-system in a different organization", OperationFailure.BadInput);
            }

            ToSystemUsage = toSystemUsage;

            if (RelationInterface != null && toSystemUsage.GetExposedInterface(RelationInterface.Id).IsNone)
            {
                RelationInterface = null;
            }
            
            return ToSystemUsage;
        }

        public Result<Maybe<ItContract.ItContract>, OperationError> SetContract(Maybe<ItContract.ItContract> contract)
        {
            var passOrganizationConstraint = 
                contract
                    .Select(x=>x.IsInSameOrganizationAs(FromSystemUsage))
                    .GetValueOrFallback(true);

            if (!passOrganizationConstraint)
            {
                return new OperationError("Attempt to create relation to it-contract in a different organization", OperationFailure.BadInput);
            }

            AssociatedContract = contract.GetValueOrDefault();
            return contract;
        }
        /// <summary>
        /// Replace relation interface
        /// </summary>
        /// <param name="targetInterface">Replacement interface to be used on the relation. NULL is allowed</param>
        public Result<Maybe<ItInterface>, OperationError> SetRelationInterface(Maybe<ItInterface> targetInterface)
        {
            if(ToSystemUsage == null)
                throw new InvalidOperationException("Cannot set interface to unknown 'To' system");

            var passExpositionConstraint =
                targetInterface
                    .Select(x => ToSystemUsage.GetExposedInterface(x.Id).HasValue)
                    .GetValueOrFallback(true);

            if (!passExpositionConstraint)
            {
                return new OperationError("Cannot set interface which is not exposed by the 'to' system", OperationFailure.BadInput);
            }

            RelationInterface = targetInterface.GetValueOrDefault();
            return targetInterface;
        }
    }
}
