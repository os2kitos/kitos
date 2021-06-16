
using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.DomainModel.ItSystem
{

    /// <summary>
    /// Represents an it system.
    /// </summary>
    public class ItSystem : ItSystemBase, IHasAccessModifier, IHierarchy<ItSystem>, IEntityWithExternalReferences, IHasAttachedOptions, IEntityWithEnabledStatus, IHasRightsHolder
    {
        public const int MaxNameLength = 100;

        public ItSystem()
        {
            ArchivePeriods = new List<ArchivePeriod>();
            ItInterfaceExhibits = new List<ItInterfaceExhibit>();
            Children = new List<ItSystem>();
            TaskRefs = new List<TaskRef>();
            AccessTypes = new List<AccessType>();
            Usages = new List<ItSystemUsage.ItSystemUsage>();
            ExternalReferences = new List<ExternalReference>();
        }

        public int? BelongsToId { get; set; }

        /// <summary>
        /// Gets or sets the organization the system belongs to.
        /// </summary>
        /// <remarks>
        /// Belongs to is a OIO term - think "produced by".
        /// </remarks>
        /// <value>
        /// The organization the it system belongs to.
        /// </value>
        public virtual Organization.Organization BelongsTo { get; set; }


        /// <summary>
        /// Gets or sets the user defined system identifier.
        /// </summary>
        /// <remarks>
        /// This identifier is NOT the primary key.
        /// </remarks>
        /// <value>
        /// The user defined system identifier.
        /// </value>
        public string ItSystemId { get; set; }

        public string PreviousName { get; set; }

        /// <summary>
        /// Gets or sets exhibited interfaces for this instance.
        /// </summary>
        /// <value>
        /// Exhibited interfaces.
        /// </value>
        public virtual ICollection<ItInterfaceExhibit> ItInterfaceExhibits { get; set; }

        /// <summary>
        /// Gets or sets the sub (child) it systems.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public virtual ICollection<ItSystem> Children { get; set; }

        public int? ParentId { get; set; }
        /// <summary>
        /// Gets or sets the parent (master) it system.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public virtual ItSystem Parent { get; set; }

        public int? BusinessTypeId { get; set; }
        /// <summary>
        /// Gets or sets the type of the business option.
        /// </summary>
        /// <value>
        /// The type of the business.
        /// </value>
        public virtual BusinessType BusinessType { get; set; }

        public virtual ICollection<ArchivePeriod> ArchivePeriods { get; set; }

        public virtual ICollection<TaskRef> TaskRefs { get; set; }

        public virtual ICollection<AccessType> AccessTypes { get; set; }

        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the usages.
        /// </summary>
        /// <value>
        /// The usages.
        /// </value>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> Usages { get; set; }

        /// <summary>
        /// Gets or sets the ExternalReferences.
        /// </summary>
        /// <value>
        /// The ExternalReferences.
        /// </value>
        public virtual ICollection<ExternalReference> ExternalReferences { get; set; }

        public ReferenceRootType GetRootType()
        {
            return ReferenceRootType.System;
        }

        public Result<ExternalReference, OperationError> AddExternalReference(ExternalReference newReference)
        {
            return new AddReferenceCommand(this).AddExternalReference(newReference);
        }

        public Result<ExternalReference, OperationError> SetMasterReference(ExternalReference newReference)
        {
            Reference = newReference;
            return newReference;
        }

        public int? ReferenceId { get; set; }

        public virtual ExternalReference Reference { get; set; }

        public ArchiveDutyRecommendationTypes? ArchiveDuty { get; set; }

        public string ArchiveDutyComment { get; set; }

        public string LinkToDirectoryAdminUrl { get; set; }
        public string LinkToDirectoryAdminUrlName { get; set; }

        public bool TryGetInterfaceExhibit(out ItInterfaceExhibit interfaceExhibit, int interfaceId)
        {
            interfaceExhibit = ItInterfaceExhibits.FirstOrDefault(i => i.ItInterface.Id == interfaceId);

            return interfaceExhibit != null;
        }

        public Maybe<ItSystemUsage.ItSystemUsage> GetUsageForOrganization(int organizationId)
        {
            return Usages.FirstOrDefault(x => x.OrganizationId == organizationId);
        }

        public Maybe<int> GetRightsHolderOrganizationId()
        {
            if (BelongsToId.HasValue)
                return BelongsToId.Value;
            if (BelongsTo != null)
                return BelongsTo.Id;
            return Maybe<int>.None;
        }
    }
}
