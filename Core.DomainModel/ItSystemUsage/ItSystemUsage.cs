﻿using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Shared;
using Core.DomainModel.Notification;
using System;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.Extensions;
using Core.DomainModel.ItSystem.DataTypes;


namespace Core.DomainModel.ItSystemUsage
{
    /// <summary>
    /// Represents an organisation's usage of an it system.
    /// </summary>
    public class ItSystemUsage :
        HasRightsEntity<ItSystemUsage, ItSystemRight, ItSystemRole>,
        ISystemModule,
        IEntityWithExternalReferences,
        IHasAttachedOptions,
        IEntityWithAdvices,
        IEntityWithUserNotification,
        IHasUuid,
        IHasDirtyMarking
    {
        public const int LongProperyMaxLength = 200;
        public const int DefaultMaxLength = 100;
        public const int LinkNameMaxLength = 150;


        public ItSystemUsage()
        {
            Contracts = new List<ItContractItSystemUsage>();
            ArchivePeriods = new List<ArchivePeriod>();
            TaskRefs = new List<TaskRef>();
            TaskRefsOptOut = new List<TaskRef>();
            UsedBy = new List<ItSystemUsageOrgUnitUsage>();
            ItProjects = new List<ItProject.ItProject>();
            ExternalReferences = new List<ExternalReference>();
            UsageRelations = new List<SystemRelation>();
            UsedByRelations = new List<SystemRelation>();
            SensitiveDataLevels = new List<ItSystemUsageSensitiveDataLevel>();
            UserNotifications = new List<UserNotification>();
            Uuid = Guid.NewGuid();
            MarkAsDirty();
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

                    // indgået-dato <= dags dato <= udløbs-dato
                    return today >= startDate.Date && today <= endDate;
                }
                return this.Active;
            }
        }

        /// <summary>
        ///     Gets or sets Active. (Enforces Active state. For more info: <see cref="IsActive"/>)
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
        public virtual ItSystem.ItSystem ItSystem { get; set; }

        public int? ArchiveTypeId { get; set; }
        public virtual ArchiveType ArchiveType { get; set; }

        public int? SensitiveDataTypeId { get; set; }
        public virtual SensitiveDataType SensitiveDataType { get; set; }

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
        /// Gets or sets the associated it projects.
        /// </summary>
        /// <remarks>
        /// <see cref="ItProject.ItProject"/> have a corresponding property linking back.
        /// </remarks>
        /// <value>
        /// Associated it projects.
        /// </value>
        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }


        public virtual ICollection<UserNotification> UserNotifications { get; set; }

        public virtual ICollection<ExternalReference> ExternalReferences { get; set; }
        public ReferenceRootType GetRootType()
        {
            return ReferenceRootType.SystemUsage;
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

        public ArchiveDutyTypes? ArchiveDuty { get; set; }

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

        public UserCount UserCount { get; set; }

        #region GDPR
        public string GeneralPurpose { get; set; }
        public DataOptions? isBusinessCritical { get; set; }


        public string LinkToDirectoryUrl { get; set; }
        public string LinkToDirectoryUrlName { get; set; }


        public virtual ICollection<ItSystemUsageSensitiveDataLevel> SensitiveDataLevels { get; set; }

        public DataOptions? precautions { get; set; }

        public IEnumerable<TechnicalPrecaution> GetTechnicalPrecautions()
        {
            if (precautionsOptionsAccessControl)
                yield return TechnicalPrecaution.AccessControl;
            if (precautionsOptionsEncryption)
                yield return TechnicalPrecaution.Encryption;
            if (precautionsOptionsLogning)
                yield return TechnicalPrecaution.Logging;
            if (precautionsOptionsPseudonomisering)
                yield return TechnicalPrecaution.Pseudonymization;
        }
        public bool precautionsOptionsEncryption { get; set; }
        public bool precautionsOptionsPseudonomisering { get; set; }
        public bool precautionsOptionsAccessControl { get; set; }
        public bool precautionsOptionsLogning { get; set; }
        public string TechnicalSupervisionDocumentationUrlName { get; set; }
        public string TechnicalSupervisionDocumentationUrl { get; set; }

        public DataOptions? UserSupervision { get; set; }
        public DateTime? UserSupervisionDate { get; set; }
        public string UserSupervisionDocumentationUrlName { get; set; }
        public string UserSupervisionDocumentationUrl { get; set; }

        public DataOptions? riskAssessment { get; set; }
        public DateTime? riskAssesmentDate { get; set; }
        public RiskLevel? preriskAssessment { get; set; }
        public string RiskSupervisionDocumentationUrlName { get; set; }
        public string RiskSupervisionDocumentationUrl { get; set; }
        public string noteRisks { get; set; }

        public DataOptions? DPIA { get; set; }
        public DateTime? DPIADateFor { get; set; }
        public string DPIASupervisionDocumentationUrlName { get; set; }
        public string DPIASupervisionDocumentationUrl { get; set; }

        public DataOptions? answeringDataDPIA { get; set; }
        public DateTime? DPIAdeleteDate { get; set; }
        public int numberDPIA { get; set; }

        public HostedAt? HostedAt { get; set; }
        #endregion


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
        /// <summary>
        /// DPAs using this system
        /// </summary>
        public virtual ICollection<DataProcessingRegistration> AssociatedDataProcessingRegistrations { get; set; }
        /// <summary>
        /// OverviewReadModels
        /// </summary>
        public virtual ICollection<ItSystemUsageOverviewReadModel> OverviewReadModels { get; set; }

        public bool HasDataProcessingAgreement() =>
            AssociatedDataProcessingRegistrations?.Any(x => x.IsAgreementConcluded == YesNoIrrelevantOption.YES) == true;


        public Result<SystemRelation, OperationError> AddUsageRelationTo(
            ItSystemUsage toSystemUsage,
            Maybe<ItInterface> relationInterface,
            string description,
            string reference,
            Maybe<RelationFrequencyType> targetFrequency,
            Maybe<ItContract.ItContract> targetContract)
        {
            if (toSystemUsage == null)
                throw new ArgumentNullException(nameof(toSystemUsage));

            var newRelation = new SystemRelation(this);

            var updateRelationResult = UpdateRelation(newRelation, toSystemUsage, description, reference, relationInterface, targetContract, targetFrequency);

            if (updateRelationResult.Failed)
            {
                return updateRelationResult.Error;
            }

            UsageRelations.Add(newRelation);

            return newRelation;
        }

        public Result<SystemRelation, OperationError> ModifyUsageRelation(
            int relationId,
            ItSystemUsage toSystemUsage,
            string changedDescription,
            string changedReference,
            Maybe<ItInterface> relationInterface,
            Maybe<ItContract.ItContract> toContract,
            Maybe<RelationFrequencyType> toFrequency)
        {
            var relationResult = GetUsageRelation(relationId);
            if (relationResult.IsNone)
            {
                return Result<SystemRelation, OperationError>.Failure(OperationFailure.BadInput);
            }

            var relation = relationResult.Value;

            return UpdateRelation(relation, toSystemUsage, changedDescription, changedReference, relationInterface, toContract, toFrequency);
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
                .FromNullable()
                .Select(system => system.ItInterfaceExhibits)
                .Select(interfaceExhibits => interfaceExhibits.Select(interfaceExhibit => interfaceExhibit.ItInterface))
                .Select(interfaces => interfaces.ToList())
                .GetValueOrFallback(new List<ItInterface>());
        }

        public Maybe<ItInterface> GetExposedInterface(int interfaceId)
        {
            return GetExposedInterfaces().FirstOrDefault(x => x.Id == interfaceId);
        }

        public bool HasExposedInterface(int interfaceId)
        {
            return GetExposedInterface(interfaceId).HasValue;
        }

        public Maybe<SystemRelation> GetUsageRelation(int relationId)
        {
            return UsageRelations.FirstOrDefault(r => r.Id == relationId);
        }
        public Maybe<SystemRelation> GetUsageRelation(Guid systemRelationUuid)
        {
            return UsageRelations.SingleOrDefault(x => x.Uuid == systemRelationUuid);
        }

        private Result<SystemRelation, OperationError> UpdateRelation(
            SystemRelation relation,
            ItSystemUsage toSystemUsage,
            string changedDescription,
            string changedReference,
            Maybe<ItInterface> relationInterface,
            Maybe<ItContract.ItContract> toContract,
            Maybe<RelationFrequencyType> toFrequency)
        {
            return relation
                .SetRelationTo(toSystemUsage)
                .Bind(_ => _.SetDescription(changedDescription))
                .Bind(_ => _.SetRelationInterface(relationInterface))
                .Bind(_ => _.SetContract(toContract))
                .Bind(_ => _.SetFrequency(toFrequency))
                .Bind(_ => _.SetReference(changedReference));
        }

        public Result<ItSystemUsageSensitiveDataLevel, OperationError> AddSensitiveDataLevel(
            SensitiveDataLevel sensitiveDataLevel)
        {
            if (SensitiveDataLevelExists(sensitiveDataLevel))
            {
                return new OperationError("Data sensitivity level already exists", OperationFailure.Conflict);
            }

            var newDataLevel = new ItSystemUsageSensitiveDataLevel()
            {
                ItSystemUsage = this,
                SensitivityDataLevel = sensitiveDataLevel
            };

            SensitiveDataLevels.Add(newDataLevel);

            return newDataLevel;
        }

        public Result<ItSystemUsageSensitiveDataLevel, OperationError> RemoveSensitiveDataLevel(
            SensitiveDataLevel sensitiveDataLevel)
        {
            if (!SensitiveDataLevelExists(sensitiveDataLevel))
            {
                return new OperationError("Data sensitivity does not exists on system usage", OperationFailure.NotFound);
            }

            var dataLevelToRemove = SensitiveDataLevels.First(x => x.SensitivityDataLevel == sensitiveDataLevel);
            SensitiveDataLevels.Remove(dataLevelToRemove);

            return dataLevelToRemove;
        }

        private bool SensitiveDataLevelExists(SensitiveDataLevel sensitiveDataLevel)
        {
            return SensitiveDataLevels.Any(x => x.SensitivityDataLevel == sensitiveDataLevel);
        }

        public Guid Uuid { get; set; }

        public Maybe<OperationError> UpdateLocalCallName(string localCallName)
        {
            if (LocalCallName != localCallName)
            {
                if (localCallName is { Length: > DefaultMaxLength })
                    return new OperationError($"{nameof(localCallName)} is too large", OperationFailure.BadInput);
                LocalCallName = localCallName;
            }
            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> UpdateLocalSystemId(string localSystemId)
        {
            if (LocalSystemId != localSystemId)
            {
                if (localSystemId is { Length: > LongProperyMaxLength })
                    return new OperationError($"{nameof(localSystemId)} is too large", OperationFailure.BadInput);
                LocalSystemId = localSystemId;
            }
            return Maybe<OperationError>.None;
        }

        public void ResetSystemCategories()
        {
            ItSystemCategoriesId = null;
        }

        public Maybe<OperationError> UpdateSystemCategories(ItSystemCategories newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            if (ItSystemCategories == null || ItSystemCategories.Id != newValue.Id)
            {
                ItSystemCategories = newValue;
            }

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> UpdateSystemVersion(string version)
        {
            if (Version != version)
            {
                if (version is { Length: > DefaultMaxLength })
                    return new OperationError($"{nameof(version)} is too large", OperationFailure.BadInput);
                Version = version;
            }
            return Maybe<OperationError>.None;
        }

        public void ResetUserCount()
        {
            UserCount = UserCount.BELOWTEN;
        }

        public Maybe<OperationError> SetExpectedUsersInterval((int lower, int? upperBound) newIntervalValue)
        {
            switch (newIntervalValue)
            {
                case (0, 9):
                    UserCount = UserCount.BELOWTEN;
                    break;
                case (10, 50):
                    UserCount = UserCount.TENTOFIFTY;
                    break;
                case (50, 100):
                    UserCount = UserCount.FIFTYTOHUNDRED;
                    break;
                case (100, null):
                case (100, int.MaxValue):
                    UserCount = UserCount.HUNDREDPLUS;
                    break;
                default:
                    return new OperationError("Invalid user count. Please refer to input documentation", OperationFailure.BadInput);
            }
            return Maybe<OperationError>.None;
        }

        public void ResetMainContract()
        {
            MainContract?.Track();
            MainContract = null;
        }

        public Maybe<OperationError> SetMainContract(ItContract.ItContract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (MainContract == null || contract.Id != MainContract.ItContractId)
            {
                if (contract.OrganizationId != OrganizationId)
                    return new OperationError("Contract must belong to same organization as this usage", OperationFailure.BadInput);

                var contractAssociation = Contracts.FirstOrDefault(c => c.ItContractId == contract.Id);

                if (contractAssociation == null)
                    return new OperationError("The provided contract is not associated with this system usage", OperationFailure.BadInput);

                ResetMainContract();
                MainContract = contractAssociation;
            }

            return Maybe<OperationError>.None;
        }

        public void ResetProjectAssociations()
        {
            ItProjects.Clear();
        }

        public Maybe<OperationError> SetProjectAssociations(IEnumerable<ItProject.ItProject> projects)
        {
            var itProjects = projects.ToList();

            if (itProjects.Select(x => x.Uuid).Distinct().Count() != itProjects.Count)
                return new OperationError("projects must not contain duplicates", OperationFailure.BadInput);

            if (itProjects.Any(project => project.OrganizationId != OrganizationId))
                return new OperationError("All projects must belong to same organization as this system usage", OperationFailure.BadInput);

            itProjects.MirrorTo(ItProjects, p => p.Uuid);

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> UpdateSystemValidityPeriod(DateTime? newValidFrom, DateTime? newValidTo)
        {
            var validFromDate = newValidFrom?.Date;
            var validToDate = newValidTo?.Date;

            if (validFromDate.HasValue && validToDate.HasValue && validFromDate.Value.Date > validToDate.Value.Date)
            {
                return new OperationError("ValidTo must equal or proceed ValidFrom", OperationFailure.BadInput);
            }

            Concluded = validFromDate;

            ExpirationDate = validToDate;

            return Maybe<OperationError>.None;
        }

        public void ResetOrganizationalUsage()
        {
            UsedBy.Clear();
            ResponsibleUsage.Track();
            ResponsibleUsage = null;
        }

        public Maybe<OperationError> UpdateOrganizationalUsage(IEnumerable<OrganizationUnit> usingOrganizationUnits, Maybe<OrganizationUnit> responsibleOrgUnit)
        {
            var organizationUnits = usingOrganizationUnits?.ToList();
            if (organizationUnits == null)
                throw new ArgumentNullException(nameof(organizationUnits));

            if (organizationUnits.Any(x => x.OrganizationId != OrganizationId))
                return new OperationError("Using Organization units must belong to the same organization as the system usage", OperationFailure.BadInput);

            if (organizationUnits.Select(x => x.Uuid).Distinct().Count() != organizationUnits.Count)
                return new OperationError("No duplicates allowed in using org units", OperationFailure.BadInput);

            if (responsibleOrgUnit.HasValue && (organizationUnits.Any(unit => unit.Uuid == responsibleOrgUnit.Value.Uuid) == false))
                return new OperationError("Responsible org unit must be one of the using organizations", OperationFailure.BadInput);

            var newOrgUnitUsages = organizationUnits.Select(organizationUnit => new ItSystemUsageOrgUnitUsage
            {
                ItSystemUsage = this,
                OrganizationUnit = organizationUnit
            }).ToList();

            newOrgUnitUsages.MirrorTo(UsedBy, usedBy => usedBy.OrganizationUnit.Uuid);

            ResponsibleUsage = responsibleOrgUnit
                .Select(orgUnit => UsedBy.Single(x => x.OrganizationUnit.Uuid == orgUnit.Uuid))
                .GetValueOrDefault();

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> UpdateKLEDeviations(IEnumerable<TaskRef> additions, IEnumerable<TaskRef> removals)
        {
            if (additions == null)
                throw new ArgumentNullException(nameof(additions));

            if (removals == null)
                throw new ArgumentNullException(nameof(removals));

            var optIn = additions.ToList();
            var optOut = removals.ToList();

            var optInIds = optIn.Select(x => x.Uuid).Distinct().ToList();
            var optOutIds = optOut.Select(x => x.Uuid).Distinct().ToList();

            if (optInIds.Count != optIn.Count)
                return new OperationError("Duplicates in KLE Additions are not allowed", OperationFailure.BadInput);

            if (optOutIds.Count != optOut.Count)
                return new OperationError("Duplicates in KLE Removals are not allowed", OperationFailure.BadInput);

            if (optOutIds.Intersect(optInIds).Any())
                return new OperationError("KLE cannot be both added and removed", OperationFailure.BadInput);

            var systemTaskRefIds = ItSystem.TaskRefs.Select(x => x.Uuid).ToHashSet();

            if (optInIds.Any(systemTaskRefIds.Contains))
                return new OperationError("Cannot ADD KLE which is already present in the system context", OperationFailure.BadInput);

            if (optOutIds.Any(id => systemTaskRefIds.Contains(id) == false))
                return new OperationError("Cannot Remove KLE which is not present in the system context", OperationFailure.BadInput);

            optIn.MirrorTo(TaskRefs, x => x.Uuid);
            optOut.MirrorTo(TaskRefsOptOut, x => x.Uuid);

            return Maybe<OperationError>.None;
        }

        public override ItSystemRight CreateNewRight(ItSystemRole role, User user)
        {
            return new ItSystemRight
            {
                Role = role,
                User = user,
                Object = this
            };
        }

        public void ResetArchiveType()
        {
            ArchiveType.Track();
            ArchiveType = null;
        }

        public Maybe<OperationError> UpdateArchiveType(ArchiveType newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            if (ArchiveType == null || ArchiveType.Id != newValue.Id)
            {
                ArchiveType = newValue;
            }

            return Maybe<OperationError>.None;
        }

        public void ResetArchiveLocation()
        {
            ArchiveLocation.Track();
            ArchiveLocation = null;
        }

        public Maybe<OperationError> UpdateArchiveLocation(ArchiveLocation newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            if (ArchiveLocation == null || ArchiveLocation.Id != newValue.Id)
            {
                ArchiveLocation = newValue;
            }

            return Maybe<OperationError>.None;
        }

        public void ResetArchiveTestLocation()
        {
            ArchiveTestLocation.Track();
            ArchiveTestLocation = null;
        }

        public Maybe<OperationError> UpdateArchiveTestLocation(ArchiveTestLocation newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            if (ArchiveTestLocation == null || ArchiveTestLocation.Id != newValue.Id)
            {
                ArchiveTestLocation = newValue;
            }

            return Maybe<OperationError>.None;
        }

        public void ResetArchiveSupplierOrganization()
        {
            //TODO: Revisit this once https://os2web.atlassian.net/browse/KITOSUDV-2118 is resolved
            ArchiveSupplier = "";
            SupplierId = null;
        }

        public Maybe<OperationError> UpdateArchiveSupplierOrganization(Organization.Organization newValue)
        {
            //TODO: Revisit this once https://os2web.atlassian.net/browse/KITOSUDV-2118 is resolved
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            ArchiveSupplier = newValue.Name;

            if (SupplierId != newValue.Id)
            {
                SupplierId = newValue.Id;
            }

            return Maybe<OperationError>.None;
        }

        public Result<IEnumerable<ArchivePeriod>, OperationError> ResetArchivePeriods()
        {
            var periodsToRemove = ArchivePeriods.ToList();
            ArchivePeriods.Clear();
            return periodsToRemove;
        }

        public Result<ArchivePeriod, OperationError> AddArchivePeriod(DateTime startDate, DateTime endDate, string archiveId, bool approved)
        {
            if(startDate.Date > endDate.Date)
                return new OperationError($"StartDate: {startDate.Date} cannot be before EndDate: {endDate.Date}", OperationFailure.BadInput);

            var newPeriod = new ArchivePeriod()
            {
                Approved = approved,
                UniqueArchiveId = archiveId,
                StartDate = startDate,
                EndDate = endDate,
                ItSystemUsage = this
            };

            ArchivePeriods.Add(newPeriod);
            return newPeriod;
        }

        public Maybe<OperationError> UpdateDataSensitivityLevels(IEnumerable<SensitiveDataLevel> sensitiveDataLevels)
        {
            if (sensitiveDataLevels == null)
                throw new ArgumentNullException(nameof(sensitiveDataLevels));

            var levels = sensitiveDataLevels.ToList();

            if (levels.Distinct().Count() != levels.Count)
                return new OperationError("Duplicate sensitivity levels are not allowed", OperationFailure.BadInput);

            var levelMappings = levels.Select(sensitiveDataLevel => new ItSystemUsageSensitiveDataLevel()
            {
                ItSystemUsage = this,
                SensitivityDataLevel = sensitiveDataLevel
            }).ToList();

            levelMappings.MirrorTo(SensitiveDataLevels, x => x.SensitivityDataLevel);

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> UpdateTechnicalPrecautions(IEnumerable<TechnicalPrecaution> technicalPrecautions)
        {
            if (technicalPrecautions == null)
                throw new ArgumentNullException(nameof(technicalPrecautions));

            var enabledPrecautions = technicalPrecautions.ToList();

            if (enabledPrecautions.Count != enabledPrecautions.Distinct().Count())
                return new OperationError("Duplicates are not allowed in technical precautions", OperationFailure.BadInput);

            var disabledPrecautions = Enum.GetValues(typeof(TechnicalPrecaution)).Cast<TechnicalPrecaution>().Except(enabledPrecautions).ToList();

            var changeSet = enabledPrecautions
                .Select(p =>
                    new
                    {
                        Enabled = true,
                        Precaution = p
                    }).Concat(disabledPrecautions.Select(p => new
                    {
                        Enabled = false,
                        Precaution = p

                    }));

            foreach (var change in changeSet)
            {
                switch (change.Precaution)
                {
                    case TechnicalPrecaution.Encryption:
                        precautionsOptionsEncryption = change.Enabled;
                        break;
                    case TechnicalPrecaution.Pseudonymization:
                        precautionsOptionsPseudonomisering = change.Enabled;
                        break;
                    case TechnicalPrecaution.AccessControl:
                        precautionsOptionsAccessControl = change.Enabled;
                        break;
                    case TechnicalPrecaution.Logging:
                        precautionsOptionsLogning = change.Enabled;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return Maybe<OperationError>.None;
        }

        public void MarkAsDirty()
        {
            LastChanged = DateTime.UtcNow;
        }
    }
}
