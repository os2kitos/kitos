
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Extensions;
using Core.DomainModel.Organization;
using Core.DomainModel.References;

namespace Core.DomainModel.ItSystem
{

    /// <summary>
    /// Represents an it system.
    /// </summary>
    public class ItSystem : ItSystemBase, IHierarchy<ItSystem>, IEntityWithExternalReferences, IEntityWithEnabledStatus, IHasRightsHolder
    {
        public const int MaxNameLength = 100;

        public ItSystem()
        {
            ItInterfaceExhibits = new List<ItInterfaceExhibit>();
            Children = new List<ItSystem>();
            TaskRefs = new List<TaskRef>();
            Usages = new List<ItSystemUsage.ItSystemUsage>();
            ExternalReferences = new List<ExternalReference>();
        }

        public int? BelongsToId { get; set; }

        public Guid? ExternalUuid { get; set; }

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

        public virtual ICollection<TaskRef> TaskRefs { get; set; }

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
        public void ClearMasterReference()
        {
            Reference.Track();
            Reference = null;
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

        public string DBSName { get; set; }

        public string DBSDataProcessorName { get; set; }

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

        public void AddTaskRef(TaskRef taskRef)
        {
            var existing = GetTaskRef(taskRef.Id);
            if (existing.IsNone)
                TaskRefs.Add(taskRef);
        }

        public void RemoveTaskRef(TaskRef taskRef)
        {
            var toRemove = GetTaskRef(taskRef.Id);
            if (toRemove.HasValue)
                TaskRefs.Remove(toRemove.Value);
        }

        public Maybe<TaskRef> GetTaskRef(int taskRefId)
        {
            return TaskRefs.SingleOrDefault(tr => tr.Id == taskRefId);
        }

        public void ResetBusinessType()
        {
            BusinessType = null;
            BusinessTypeId = null;
        }

        public void UpdateBusinessType(BusinessType businessType)
        {
            if (businessType == null)
                throw new ArgumentNullException(nameof(businessType));

            BusinessType = businessType;
            BusinessTypeId = businessType.Id;
        }

        public void ResetParentSystem()
        {
            Parent = null;
            ParentId = null;
        }

        public void UpdateParentSystem(ItSystem parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            Parent = parent;
            ParentId = parent.Id;
        }

        public void ResetRightsHolder()
        {
            BelongsTo = null;
            BelongsToId = null;
        }

        public void UpdateRightsHolder(Organization.Organization organization)
        {
            if (organization == null)
                throw new ArgumentNullException(nameof(organization));

            BelongsTo = organization;
            BelongsToId = organization.Id;
        }

        public static bool IsValidName(string newName)
        {
            return string.IsNullOrWhiteSpace(newName) == false && newName.Length <= MaxNameLength;
        }

        public Maybe<OperationError> UpdateName(string newName)
        {
            if (!IsValidName(newName))
                return new OperationError("Invalid name", OperationFailure.BadInput);
            Name = newName;
            return Maybe<OperationError>.None;
        }

        public void Deactivate()
        {
            Disabled = true;
        }

        public void Activate()
        {
            Disabled = false;
        }

        public void ChangeOrganization(Organization.Organization newOrganization)
        {
            if (newOrganization == null)
            {
                throw new ArgumentNullException(nameof(newOrganization));
            }
            Organization = newOrganization;
            OrganizationId = newOrganization.Id;
        }

        public void RemoveChildSystem(ItSystem child)
        {
            if (child.ParentId == Id)
            {
                Children.Remove(child);
                child.ResetParentSystem();
            }
            else
            {
                throw new ArgumentException(nameof(child));
            }
        }

        public Maybe<OperationError> UpdateRecommendedArchiveDuty(ArchiveDutyRecommendationTypes? recommendation, string comment)
        {
            var noCommentAllowed = recommendation is null or ArchiveDutyRecommendationTypes.Undecided;
            if (noCommentAllowed && !string.IsNullOrEmpty(comment))
            {
                return new OperationError("Comment should only be defined if an actual recommendation exists", OperationFailure.BadInput);
            }

            ArchiveDuty = recommendation;
            ArchiveDutyComment = comment;
            return Maybe<OperationError>.None;
        }

        public void UpdateDBSName(string dbsName)
        {
            DBSName = dbsName;
        }

        public void UpdateDBSDataProcessorName(string dbsDataProcessorName)
        {
            DBSDataProcessorName = dbsDataProcessorName;
        }
    }
}
