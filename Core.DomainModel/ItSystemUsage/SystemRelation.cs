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
        public Result<SystemRelation, OperationError> SetRelationTo(ItSystemUsage toSystemUsage)
        {
            if (toSystemUsage == null)
                throw new ArgumentNullException(nameof(toSystemUsage));


            if (FromSystemUsage.Id == toSystemUsage.Id)
            {
                return new OperationError("'From' cannot equal 'To'", OperationFailure.BadInput);
            }

            //Both ends of the relation must be in same organization
            if (!CheckSameOrganizationConstraint<ItSystemUsage>(toSystemUsage).GetValueOrFallback(false))
            {
                return new OperationError("Attempt to create relation to it-system in a different organization", OperationFailure.BadInput);
            }

            ToSystemUsage = toSystemUsage;

            RelationInterface =
                RelationInterface
                    .FromNullable()
                    .Select(relationInterface => ToSystemUsage.GetExposedInterface(relationInterface.Id).HasValue)
                    .GetValueOrFallback(false)
                    ? RelationInterface
                    : null;

            return this;
        }

        public Result<SystemRelation, OperationError> SetContract(Maybe<ItContract.ItContract> contract)
        {
            //IF contract is defined it MUST be in the same organization
            if (!CheckSameOrganizationConstraint(contract).GetValueOrFallback(true))
            {
                return new OperationError("Attempt to create relation to it-contract in a different organization", OperationFailure.BadInput);
            }

            AssociatedContract = contract.GetValueOrDefault();
            return this;
        }

        private Maybe<bool> CheckSameOrganizationConstraint<T>(Maybe<T> other) where T : IHasOrganization
        {
            return other.Select(x => x.IsInSameOrganizationAs(FromSystemUsage));
        }

        /// <summary>
        /// Replace relation interface
        /// </summary>
        /// <param name="relationInterface">Replacement interface to be used on the relation. None is allowed</param>
        public Result<SystemRelation, OperationError> SetRelationInterface(Maybe<ItInterface> relationInterface)
        {
            if (ToSystemUsage == null)
                throw new InvalidOperationException("Cannot set interface to unknown 'To' system");

            if (relationInterface.HasValue)
            {
                if (!ToSystemUsage.HasExposedInterface(relationInterface.Value.Id))
                {
                    return new OperationError("Cannot set interface which is not exposed by the 'to' system", OperationFailure.BadInput);
                }
            }

            RelationInterface = relationInterface.GetValueOrDefault();

            return this;
        }

        /// <summary>
        /// Replaces frequency type
        /// </summary>
        /// <param name="toFrequency">Replacement frequency to be used in relation. NULL is allowed</param>
        /// <returns></returns>
        public Result<SystemRelation, OperationError> SetFrequency(Maybe<RelationFrequencyType> toFrequency)
        {
            UsageFrequency = toFrequency.GetValueOrDefault();

            return this;
        }

        public Result<SystemRelation, OperationError> SetReference(string changedReference)
        {
            Reference = changedReference;

            return this;
        }

        public Result<SystemRelation, OperationError> SetDescription(string changedDescription)
        {
            Description = changedDescription;

            return this;
        }
    }
}
