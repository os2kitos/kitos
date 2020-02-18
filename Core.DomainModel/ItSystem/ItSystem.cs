using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainModel.References;

namespace Core.DomainModel.ItSystem
{
    using DataTypes;

    /// <summary>
    /// Represents an it system.
    /// </summary>
    public class ItSystem : ItSystemBase, IHasAccessModifier, IHierarchy<ItSystem>, IEntityWithExternalReferences
    {
        public ItSystem()
        {
            this.ArchivePeriods = new List<ArchivePeriod>();
            this.ItInterfaceExhibits = new List<ItInterfaceExhibit>();
            this.Children = new List<ItSystem>();
            this.TaskRefs = new List<TaskRef>();
            this.AccessTypes = new List<AccessType>();
            this.Wishes = new List<Wish>();
            this.Usages = new List<ItSystemUsage.ItSystemUsage>();
            ExternalReferences = new List<ExternalReference>();
            this.AssociatedDataWorkers = new List<ItSystemDataWorkerRelation>();

        }

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

        public int? AppTypeOptionId { get; set; }
        public virtual ItSystemType AppTypeOption { get; set; }

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

        public virtual ICollection<Wish> Wishes { get; set; }
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

        public int? ReferenceId { get; set; }

        public virtual ExternalReference Reference { get; set; }
        //GDPR
        public string GeneralPurpose { get; set; }

        public DataSensitivityLevel DataLevel { get; set; }

        public DataOptions ContainsLegalInfo { get; set; }

        public bool IsDataTransferedToThirdCountries { get; set; }

        public string DataIsTransferedTo { get; set; }

        public int ArchiveDuty { get; set; }

        public virtual ICollection<ItSystemDataWorkerRelation> AssociatedDataWorkers { get; set; }

        public string LinkToDirectoryAdminUrl { get; set; }
        public string LinkToDirectoryAdminUrlName { get; set; }

        public bool TryGetInterfaceExhibit(out ItInterfaceExhibit interfaceExhibit, int interfaceId)
        {
            interfaceExhibit = ItInterfaceExhibits.FirstOrDefault(i => i.ItInterface.Id == interfaceId);

            return interfaceExhibit != null;
        }
    }
}
