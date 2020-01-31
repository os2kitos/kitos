using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Extensions;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;

namespace Core.DomainModel.ItSystemUsage
{
    using ItSystem.DataTypes;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    using ItSystem = Core.DomainModel.ItSystem.ItSystem;

    /// <summary>
    /// Represents an organisation's usage of an it system.
    /// </summary>
    public class ItSystemUsage : HasRightsEntity<ItSystemUsage, ItSystemRight, ItSystemRole>, IContextAware, ISystemModule, IHasOrganization, IEntityWithExternalReferences
    {
        public ItSystemUsage()
        {
            this.Contracts = new List<ItContractItSystemUsage>();
            this.Wishes = new List<Wish>();
            this.ArchivePeriods = new List<ArchivePeriod>();
            this.OrgUnits = new List<OrganizationUnit>();
            this.TaskRefs = new List<TaskRef>();
            this.AccessTypes = new List<AccessType>();
            this.TaskRefsOptOut = new List<TaskRef>();
            this.ItInterfaceUsages = new List<ItInterfaceUsage>();
            this.ItInterfaceExhibitUsages = new List<ItInterfaceExhibitUsage>();
            this.UsedBy = new List<ItSystemUsageOrgUnitUsage>();
            this.ItProjects = new List<ItProject.ItProject>();
            ExternalReferences = new List<ExternalReference>();
            this.AssociatedDataWorkers = new List<ItSystemUsageDataWorkerRelation>();
            UsageRelations = new List<SystemRelation>();
            UsedByRelations = new List<SystemRelation>();
        }

        public bool IsActive
        {
            get
            {
                if (!this.Active)
                {
                    var today = DateTime.UtcNow;
                    var startDate = this.Concluded ?? today;
                    var endDate = DateTime.MaxValue;

                    if (ExpirationDate.HasValue && ExpirationDate.Value != DateTime.MaxValue)
                    {
                        endDate = ExpirationDate.Value.Date.AddDays(1).AddTicks(-1);
                    }

                    if (this.Terminated.HasValue)
                    {
                        var terminationDate = this.Terminated;
                        if (this.TerminationDeadlineInSystem != null)
                        {
                            int deadline;
                            int.TryParse(this.TerminationDeadlineInSystem.Name, out deadline);
                            terminationDate = this.Terminated.Value.AddMonths(deadline);
                        }
                        // indgået-dato <= dags dato <= opsagt-dato + opsigelsesfrist
                        return today >= startDate.Date && today <= terminationDate.Value.Date.AddDays(1).AddTicks(-1);
                    }

                    // indgået-dato <= dags dato <= udløbs-dato
                    return today >= startDate.Date && today <= endDate;
                }
                return this.Active;
            }
        }

        /// <summary>
        ///     Gets or sets Active.
        /// </summary>
        /// <value>
        ///   Active.
        /// </value>
        public bool Active { get; set; }

        /// <summary>
        ///     When the system began. (indgået)
        /// </summary>
        /// <value>
        ///     The concluded date.
        /// </value>
        public DateTime? Concluded { get; set; }

        /// <summary>
        ///     When the system expires. (udløbet)
        /// </summary>
        /// <value>
        ///     The expiration date.
        /// </value>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        ///     Date the system ends. (opsagt)
        /// </summary>
        /// <value>
        ///     The termination date.
        /// </value>
        public DateTime? Terminated { get; set; }

        /// <summary>
        ///     Gets or sets the termination deadline option. (opsigelsesfrist)
        /// </summary>
        /// <remarks>
        ///     Added months to the <see cref="Terminated" /> contract termination date before the contract expires.
        ///     It's a string but should be treated as an int.
        ///     TODO perhaps a redesign of OptionEntity is in order
        /// </remarks>
        /// <value>
        ///     The termination deadline.
        /// </value>
        public virtual TerminationDeadlineTypesInSystem TerminationDeadlineInSystem { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance's status is active.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance's status is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsStatusActive { get; set; }
        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public string Note { get; set; }
        /// <summary>
        /// Gets or sets the user defined local system identifier.
        /// </summary>
        /// <remarks>
        /// This identifier is not the primary key.
        /// </remarks>
        /// <value>
        /// The local system identifier.
        /// </value>
        public string LocalSystemId { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }
        /// <summary>
        /// Gets or sets a reference to relevant documents in an extern ESDH system.
        /// </summary>
        /// <value>
        /// Extern reference  to ESDH system.
        /// </value>
        public string EsdhRef { get; set; }
        /// <summary>
        /// Gets or sets a reference to relevant documents in an extern CMDB system.
        /// </summary>
        /// <value>
        /// Extern reference  to CMDB system.
        /// </value>
        public string CmdbRef { get; set; }
        /// <summary>
        /// Gets or sets a path or url to relevant documents.
        /// </summary>
        /// <value>
        /// Path or url relevant documents.
        /// </value>
        public string DirectoryOrUrlRef { get; set; }
        /// <summary>
        /// Gets or sets the local call system.
        /// </summary>
        /// <value>
        /// The local call system.
        /// </value>
        public string LocalCallName { get; set; }
        /// <summary>
        /// Organization Unit responsible for this system usage.
        /// </summary>
        /// <value>
        /// The responsible organization unit.
        /// </value>
        public virtual ItSystemUsageOrgUnitUsage ResponsibleUsage { get; set; }

        public int OrganizationId { get; set; }
        /// <summary>
        /// Gets or sets the organization marked as responsible for this it system usage.
        /// </summary>
        /// <value>
        /// The responsible organization.
        /// </value>
        public virtual Organization.Organization Organization { get; set; }

        public int ItSystemId { get; set; }
        /// <summary>
        /// Gets or sets the it system this instance is using.
        /// </summary>
        /// <value>
        /// It system.
        /// </value>
        public virtual ItSystem ItSystem { get; set; }

        public int? ArchiveTypeId { get; set; }
        public virtual ArchiveType ArchiveType { get; set; }

        public int? SensitiveDataTypeId { get; set; }
        public virtual SensitiveDataType SensitiveDataType { get; set; }

        public int? OverviewId { get; set; }
        /// <summary>
        /// Gets or sets the it system usage that is set to be displayed on the it system overview page.
        /// </summary>
        /// <remarks>
        /// It's the it system name that is actually displayed.
        /// </remarks>
        /// <value>
        /// The overview it system.
        /// </value>
        public virtual ItSystemUsage Overview { get; set; }

        /// <summary>
        /// Gets or sets the main it contract for this instance.
        /// The it contract is used to determine whether this instance
        /// is marked as active/inactive.
        /// </summary>
        /// <value>
        /// The main contract.
        /// </value>
        public virtual ItContractItSystemUsage MainContract { get; set; }
        /// <summary>
        /// Gets or sets it contracts associated with this instance.
        /// </summary>
        /// <value>
        /// The contracts.
        /// </value>
        public virtual ICollection<ItContractItSystemUsage> Contracts { get; set; }
        /// <summary>
        /// Gets or sets the wishes associated with this instance.
        /// </summary>
        /// <value>
        /// Wishes.
        /// </value>
        public virtual ICollection<Wish> Wishes { get; set; }
        /// <summary>
        /// Gets or sets the organization units associated with this instance.
        /// </summary>
        /// <value>
        /// The organization units.
        /// </value>
        public virtual ICollection<OrganizationUnit> OrgUnits { get; set; }
        /// <summary>
        /// Gets or sets the organization units that are using this instance.
        /// </summary>
        /// <remarks>
        /// Must be organization units that belongs to <see cref="Organization"/>.
        /// </remarks>
        /// <value>
        /// The organization units used by this instance.
        /// </value>
        public virtual ICollection<ItSystemUsageOrgUnitUsage> UsedBy { get; set; }
        /// <summary>
        /// Gets or sets the tasks this instance supports.
        /// </summary>
        /// <value>
        /// The supported tasks.
        /// </value>
        public virtual ICollection<TaskRef> TaskRefs { get; set; }
        /// <summary>
        /// Gets or sets the tasks that has been opted out from by an organization.
        /// </summary>
        public virtual ICollection<TaskRef> TaskRefsOptOut { get; set; }
        /// <summary>
        /// The local usages of interfaces.
        /// </summary>
        public virtual ICollection<ItInterfaceUsage> ItInterfaceUsages { get; set; }
        /// <summary>
        /// The local exposures of interfaces.
        /// </summary>
        public virtual ICollection<ItInterfaceExhibitUsage> ItInterfaceExhibitUsages { get; set; }
        /// <summary>
        /// Gets or sets the associated it projects.
        /// </summary>
        /// <remarks>
        /// <see cref="ItProject.ItProject"/> have a corresponding property linking back.
        /// </remarks>
        /// <value>
        /// Associated it projects.
        /// </value>
        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }

        public virtual ICollection<ExternalReference> ExternalReferences { get; set; }

        public int? ReferenceId { get; set; }
        public virtual ExternalReference Reference { get; set; }
        public virtual ICollection<AccessType> AccessTypes { get; set; }

        /// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            return OrganizationId == organizationId;
        }


        public int? ArchiveDuty { get; set; }

        public bool? Archived { get; set; }

        public bool? ReportedToDPA { get; set; }

        public string DocketNo { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ArchivedDate { get; set; }

        public string ArchiveNotes { get; set; }

        public int? ArchiveFreq { get; set; }

        public string ArchiveSupplier { get; set; }

        public bool? Registertype { get; set; }

        public int? SupplierId { get; set; }
        /// <summary>
        ///     Gets or sets the organization marked as supplier for this contract.
        /// </summary>
        /// <value>
        ///     The organization.
        /// </value>
        public int? ArchiveLocationId { get; set; }

        public virtual ArchiveLocation ArchiveLocation { get; set; }

        public int? ArchiveTestLocationId { get; set; }

        public virtual ArchiveTestLocation ArchiveTestLocation { get; set; }

        public int? ItSystemCategoriesId { get; set; }

        public virtual ItSystemCategories ItSystemCategories { get; set; }

        public string GeneralPurpose { get; set; }
        public DataOptions isBusinessCritical { get; set; }
        public DataOptions ContainsLegalInfo { get; set; }
        public DataSensitivityLevel DataLevel { get; set; }
        public UserCount UserCount { get; set; }

        public string systemCategories { get; set; }

        public string dataProcessor { get; set; }

        public int dataProcessorControl { get; set; }

        public DateTime? lastControl { get; set; }

        public string noteUsage { get; set; }

        public int precautions { get; set; }

        public int riskAssessment { get; set; }

        public DateTime? riskAssesmentDate { get; set; }

        public int preriskAssessment { get; set; }

        public string noteRisks { get; set; }

        public int DPIAhearing { get; set; }

        public DateTime? DPIADate { get; set; }

        public int DPIA { get; set; }

        public DateTime? DPIADateFor { get; set; }

        public int answeringDataDPIA { get; set; }

        public DateTime? DPIAdeleteDate { get; set; }

        public int numberDPIA { get; set; }

        public bool precautionsOptionsEncryption { get; set; }
        public bool precautionsOptionsPseudonomisering { get; set; }
        public bool precautionsOptionsAccessControl { get; set; }
        public bool precautionsOptionsLogning { get; set; }

        public virtual ICollection<ItSystemUsageDataWorkerRelation> AssociatedDataWorkers { get; set; }

        public string datahandlerSupervisionDocumentationUrlName { get; set; }
        public string datahandlerSupervisionDocumentationUrl { get; set; }

        public string TechnicalSupervisionDocumentationUrlName { get; set; }
        public string TechnicalSupervisionDocumentationUrl { get; set; }

        public string UserSupervisionDocumentationUrlName { get; set; }
        public string UserSupervisionDocumentationUrl { get; set; }

        public string RiskSupervisionDocumentationUrlName { get; set; }
        public string RiskSupervisionDocumentationUrl { get; set; }

        public string DPIASupervisionDocumentationUrlName { get; set; }
        public string DPIASupervisionDocumentationUrl { get; set; }

        public string DataHearingSupervisionDocumentationUrlName { get; set; }
        public string DataHearingSupervisionDocumentationUrl { get; set; }

        public DateTime? UserSupervisionDate { get; set; }

        public int UserSupervision { get; set; }
        public string LinkToDirectoryUrl { get; set; }
        public string LinkToDirectoryUrlName { get; set; }

        public virtual ICollection<ArchivePeriod> ArchivePeriods { get; set; }

        public bool? ArchiveFromSystem { get; set; }
        /// <summary>
        /// Defines how this system uses other systems.
        /// </summary>
        public virtual ICollection<SystemRelation> UsageRelations { get; set; }
        /// <summary>
        /// Defines how this system is used by other systems
        /// </summary>
        public virtual ICollection<SystemRelation> UsedByRelations { get; set; }

        public Result<SystemRelation, OperationError> AddUsageRelationTo(
            User activeUser,
            ItSystemUsage destination,
            int? interfaceId,
            string description,
            string reference,
            Maybe<RelationFrequencyType> targetFrequency,
            Maybe<ItContract.ItContract> targetContract)
        {
            if (activeUser == null)
                throw new ArgumentNullException(nameof(activeUser));

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            if (Id == destination.Id)
                return new OperationError("Cannot create relation to self", OperationFailure.BadInput);

            if (!this.IsInSameOrganizationAs(destination))
                return new OperationError("Attempt to create relation to it-system in a different organization", OperationFailure.BadInput);

            var allowContractBinding =
                targetContract
                    .Select(this.IsInSameOrganizationAs)
                    .GetValueOrFallback(true);

            if (!allowContractBinding)
                return new OperationError("Attempt to create relation to it-contract in a different organization", OperationFailure.BadInput);

            var exposedInterface = Maybe<ItInterface>.None;
            if (interfaceId.HasValue)
            {

                exposedInterface = destination.GetExposedInterface(interfaceId.Value);
                if (!exposedInterface.HasValue)
                    return new OperationError("Interface is not exposed by the target system", OperationFailure.BadInput);
            }

            var newRelation = new SystemRelation(this, destination)
            {
                Description = description,
                AssociatedContract = targetContract.GetValueOrDefault(),
                RelationInterface = exposedInterface.GetValueOrDefault(),
                UsageFrequency = targetFrequency.GetValueOrDefault(),
                Reference = reference,
                ObjectOwner = ObjectOwner,
                LastChangedByUser = activeUser,
                LastChanged = DateTime.Now
            };

            UsageRelations.Add(newRelation);

            LastChangedByUser = activeUser;
            LastChanged = DateTime.Now;

            return newRelation;
        }

        public Result<SystemRelation, OperationError> ModifyUsageRelation(User activeUser, int sourceSystemRelationId,
            Maybe<ItSystemUsage> targetSystemUsage, Maybe<ItInterface> targetInterface)
        {
            if (activeUser == null)
            {
                throw new ArgumentNullException(nameof(activeUser));
            }

            var relation = UsageRelations.FirstOrDefault(r => r.Id == sourceSystemRelationId);
            if (relation == null)
            {
                return Result<SystemRelation, OperationError>.Failure(OperationFailure.BadInput);
            }

            if (targetSystemUsage.HasValue)
            {
                relation.SetRelationTarget(targetSystemUsage.Value);
            }

            if (targetInterface.HasValue)
            {
                relation.SetRelationInterface(targetInterface.Value);
            }

            return relation;
        }

        public Result<SystemRelation, OperationFailure> RemoveUsageRelation(int relationId)
        {
            var relationResult = GetUsageRelation(relationId);

            if (!relationResult.HasValue)
            {
                return OperationFailure.NotFound;
            }

            var relation = relationResult.Value;
            UsageRelations.Remove(relation);
            return relation;
        }
		
		public IEnumerable<ItInterface> GetExposedInterfaces()
        {
            return ItSystem
                .ItInterfaceExhibits
                .Select(x => x.ItInterface)
                .ToList();
		}

        public Maybe<ItInterface> GetExposedInterface(int interfaceId)
        {
			return GetExposedInterfaces().FirstOrDefault(x => x.Id == interfaceId);
        }

        public Maybe<SystemRelation> GetUsageRelation(int relationId)
        {
            return UsageRelations.FirstOrDefault(r => r.Id == relationId);
        }
    }
}
