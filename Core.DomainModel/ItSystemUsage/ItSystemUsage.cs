using System.Collections.Generic;
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
            ExternalReferences = new List<ExternalReference>();
            UsageRelations = new List<SystemRelation>();
            UsedByRelations = new List<SystemRelation>();
            SensitiveDataLevels = new List<ItSystemUsageSensitiveDataLevel>();
            UserNotifications = new List<UserNotification>();
            Uuid = Guid.NewGuid();
            MarkAsDirty();
            AssociatedDataProcessingRegistrations = new List<DataProcessingRegistration>();
            PersonalDataOptions = new List<ItSystemUsagePersonalData>();
        }

        public bool IsActiveAccordingToDateFields => CheckDatesValidity(DateTime.UtcNow).Any() == false;
        public bool IsActiveAccordingToLifeCycle => CheckLifeCycleValidity().IsNone;
        public bool IsActiveAccordingToMainContract => CheckContractValidity().IsNone;

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
        /// Gets or sets the life cycle status of this system usage.
        /// </summary>
        /// <value>
        /// The life cycle status type of the system.
        /// </value>
        public LifeCycleStatusType? LifeCycleStatus { get; set; }

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

        public bool? Registertype { get; set; }

        public int? ArchiveSupplierId { get; set; }
        public virtual Organization.Organization ArchiveSupplier { get; set; }

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

        public UserCount? UserCount { get; set; }

        #region GDPR
        public string GeneralPurpose { get; set; }
        public DataOptions? isBusinessCritical { get; set; }


        public string LinkToDirectoryUrl { get; set; }
        public string LinkToDirectoryUrlName { get; set; }


        public virtual ICollection<ItSystemUsagePersonalData> PersonalDataOptions { get; set; }
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
        public DateTime? PlannedRiskAssessmentDate { get; set; }
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

        public Result<RemoveSensitiveDataLevelResultModel, OperationError> RemoveSensitiveDataLevel(
            SensitiveDataLevel sensitiveDataLevel)
        {
            if (!SensitiveDataLevelExists(sensitiveDataLevel))
            {
                return new OperationError("Data sensitivity does not exists on system usage", OperationFailure.NotFound);
            }

            var dataLevelToRemove = SensitiveDataLevels.First(x => x.SensitivityDataLevel == sensitiveDataLevel);

            var removedPersonalData = new List<ItSystemUsagePersonalData>();
            if (dataLevelToRemove.SensitivityDataLevel == SensitiveDataLevel.PERSONALDATA)
            {
                removedPersonalData.AddRange(ResetPersonalData());
            }
            SensitiveDataLevels.Remove(dataLevelToRemove);

            return new RemoveSensitiveDataLevelResultModel(dataLevelToRemove, removedPersonalData);
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
            UserCount = DomainModel.ItSystem.DataTypes.UserCount.UNDECIDED;
        }

        public Maybe<OperationError> SetExpectedUsersInterval((int lower, int? upperBound) newIntervalValue)
        {
            switch (newIntervalValue)
            {
                case (0, 9):
                    UserCount = DomainModel.ItSystem.DataTypes.UserCount.BELOWTEN;
                    break;
                case (10, 50):
                    UserCount = DomainModel.ItSystem.DataTypes.UserCount.TENTOFIFTY;
                    break;
                case (50, 100):
                    UserCount = DomainModel.ItSystem.DataTypes.UserCount.FIFTYTOHUNDRED;
                    break;
                case (100, null):
                case (100, int.MaxValue):
                    UserCount = DomainModel.ItSystem.DataTypes.UserCount.HUNDREDPLUS;
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

            var responsibleOrgUnitIsValid = responsibleOrgUnit.Select(responsible => organizationUnits.Any(unit => responsible == unit)).GetValueOrFallback(true);
            if (!responsibleOrgUnitIsValid)
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

        private Maybe<ItSystemUsageOrgUnitUsage> GetOrganizationUnitUsage(int organizationUnitId)
        {
            return UsedBy.FirstOrDefault(ub => ub.OrganizationUnit.Id == organizationUnitId);
        }

        public Maybe<OperationError> RemoveResponsibleOrganizationUnit()
        {
            return UpdateOrganizationalUsage(GetUsedByOrganizationUnits(), Maybe<OrganizationUnit>.None);
        }

        public Maybe<OperationError> RemoveUsedByUnit(Guid unitUuid)
        {
            var unitResult = GetOrganizationUnit(unitUuid);
            if (unitResult.IsNone)
            {
                return new OperationError($"Organization unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);
            }
            var unit = unitResult.Value;

            var selectedUnit = GetOrganizationUnitUsage(unit.Id);
            if (selectedUnit.IsNone)
            {
                return new OperationError($"Unit with id: {unit.Id} was not found", OperationFailure.NotFound);
            }

            var remainingUnits = GetUsedByOrganizationUnits().Where(x => x.Id != unit.Id);
            Maybe<OrganizationUnit> responsibleUnit = ResponsibleUsage?.OrganizationUnit;

            if (responsibleUnit.Select(organizationUnit => organizationUnit.Id == unit.Id).GetValueOrFallback(false))
            {
                responsibleUnit = Maybe<OrganizationUnit>.None;
            }

            return UpdateOrganizationalUsage(remainingUnits, responsibleUnit);
        }

        public Maybe<OperationError> TransferResponsibleOrganizationalUnit(Guid targetUnitUuid)
        {
            var targetUnitResult = GetOrganizationUnit(targetUnitUuid);
            if (targetUnitResult.IsNone)
            {
                return new OperationError($"Organization unit with uuid: {targetUnitUuid} not found", OperationFailure.NotFound);
            }
            var targetUnit = targetUnitResult.Value;

            if (targetUnit.Id == ResponsibleUsage?.OrganizationUnitId)
            {
                return Maybe<OperationError>.None;
            }

            var usedByOrganizationUnits = GetUsedByOrganizationUnits().ToList();

            if (usedByOrganizationUnits.Any(x => x.Id == targetUnit.Id) == false)
            {
                usedByOrganizationUnits.Add(targetUnit);
            }

            return UpdateOrganizationalUsage(usedByOrganizationUnits, targetUnit);
        }

        public Maybe<OperationError> TransferUsedByUnit(Guid unitUuid, Guid targetUnitUuid)
        {
            if (unitUuid == targetUnitUuid)
            {
                return Maybe<OperationError>.None;
            }

            var targetUnitResult = GetOrganizationUnit(targetUnitUuid);
            if (targetUnitResult.IsNone)
            {
                return new OperationError($"Organization unit with uuid: {targetUnitUuid} was not found", OperationFailure.NotFound);
            }
            var targetUnit = targetUnitResult.Value;

            var unitResult = GetOrganizationUnit(unitUuid);
            if (unitResult.IsNone)
            {
                return new OperationError($"Organization unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);
            }
            var unit = unitResult.Value;

            var selectedUnit = GetOrganizationUnitUsage(unit.Id);
            if (selectedUnit.IsNone)
                return new OperationError($"UsedBy organization unit with Id: {unit.Id} was not found", OperationFailure.NotFound);

            var relevantUnits = GetUsedByOrganizationUnits().Where(x => x.Id != unit.Id).ToList();
            if (relevantUnits.Any(x => x.Id == targetUnit.Id) == false)
            {
                relevantUnits.Add(targetUnit);
            }

            if (unit.Id == ResponsibleUsage?.OrganizationUnit?.Id)
            {
                return UpdateOrganizationalUsage(relevantUnits, Maybe<OrganizationUnit>.None);
            }

            return UpdateOrganizationalUsage(relevantUnits, ResponsibleUsage?.OrganizationUnit);
        }

        public Maybe<OperationError> UpdateKLEDeviations(IEnumerable<TaskRef> additions, IEnumerable<TaskRef> removals)
        {
            if (additions == null)
                throw new ArgumentNullException(nameof(additions));

            if (removals == null)
                throw new ArgumentNullException(nameof(removals));

            var optInTaskRefs = additions.ToList();
            var optOutTaskRefs = removals.ToList();

            return WithValidKLEChanges(optInTaskRefs, optOutTaskRefs)
                .Match(e => e,
                    () =>
                    {
                        optInTaskRefs.MirrorTo(TaskRefs, x => x.Uuid);
                        optOutTaskRefs.MirrorTo(TaskRefsOptOut, x => x.Uuid);

                        return Maybe<OperationError>.None;
                    });
        }

        private Maybe<OperationError> WithValidKLEChanges(List<TaskRef> optInTaskRefs, List<TaskRef> optOutTaskRefs)
        {
            var optInIds = GetUniqueTaskRefUuids(optInTaskRefs);
            var optOutIds = GetUniqueTaskRefUuids(optOutTaskRefs);

            if (optInIds.Count != optInTaskRefs.Count)
                return new OperationError("Duplicates in KLE Additions are not allowed", OperationFailure.BadInput);

            if (optOutIds.Count != optOutTaskRefs.Count)
                return new OperationError("Duplicates in KLE Removals are not allowed", OperationFailure.BadInput);

            if (optOutIds.Intersect(optInIds).Any())
                return new OperationError("KLE cannot be both added and removed", OperationFailure.BadInput);

            var systemTaskRefIds = GetUniqueTaskRefUuids(ItSystem.TaskRefs);

            var newOptInTaskRefs = GetNewTaskRefs(optInTaskRefs);

            if (newOptInTaskRefs.Any(taskRef => systemTaskRefIds.Contains(taskRef.Uuid)))
                return new OperationError("Cannot Add KLE which is already present in the system context", OperationFailure.BadInput);

            return optOutIds.Any(id => systemTaskRefIds.Contains(id) == false) 
                ? new OperationError("Cannot Remove KLE which is not present in the system context", OperationFailure.BadInput) 
                : Maybe<OperationError>.None;
        }

        private HashSet<Guid> GetUniqueTaskRefUuids(IEnumerable<TaskRef> taskRefs)
        {
            return taskRefs.Select(x => x.Uuid).ToHashSet();
        }

        private IEnumerable<TaskRef> GetNewTaskRefs(List<TaskRef> taskRefs)
        {
            return taskRefs.Where(taskRef => TaskRefs.All(tr => tr.Uuid != taskRef.Uuid));
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
            ArchiveSupplier.Track();
            ArchiveSupplier = null;
        }

        public Maybe<OperationError> UpdateArchiveSupplierOrganization(Organization.Organization newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            if (ArchiveSupplier == null || ArchiveSupplier.Id != newValue.Id)
            {
                ArchiveSupplier = newValue;
            }

            return Maybe<OperationError>.None;
        }

        public Result<IEnumerable<ArchivePeriod>, OperationError> ResetArchivePeriods()
        {
            var periodsToRemove = ArchivePeriods.ToList();
            ArchivePeriods.Clear();
            return periodsToRemove;
        }

        public Result<ArchivePeriod, OperationError> RemoveArchivePeriod(Guid archivePeriodUuid)
        {
            var archivePeriodResult = GetArchivePeriod(archivePeriodUuid);
            if(archivePeriodResult.IsNone)
                return new OperationError($"Could not find existing period with uuid {archivePeriodUuid}", OperationFailure.NotFound);

            var archivePeriod = archivePeriodResult.Value;
            ArchivePeriods.Remove(archivePeriod);
            return archivePeriod;
        }

        public Result<ArchivePeriod, OperationError> AddArchivePeriod(DateTime startDate, DateTime endDate, string archiveId, bool approved)
        {
            var newPeriod = new ArchivePeriod
            {
                ItSystemUsage = this
            };

            var error = UpdateArchivePeriod(newPeriod, startDate, endDate, archiveId, approved);
            if (error.HasValue)
                return error.Value;

            ArchivePeriods.Add(newPeriod);
            return newPeriod;
        }

        public Result<ArchivePeriod, OperationError> UpdateArchivePeriod(Guid archivePeriodUuid, DateTime startDate, DateTime endDate, string archiveId, bool approved)
        {
            var periodResult = GetArchivePeriod(archivePeriodUuid);
            if (periodResult.IsNone)
                return new OperationError($"Could not find existing period with uuid {archivePeriodUuid}", OperationFailure.NotFound);

            return UpdateArchivePeriod(periodResult.Value, startDate, endDate, archiveId, approved)
                .Match<Result<ArchivePeriod, OperationError>>
                (
                    error => error,
                    () => periodResult.Value
                );
        }

        public Maybe<ArchivePeriod> GetArchivePeriod(Guid archivePeriodUuid)
        {
            return ArchivePeriods.FirstOrDefault(x => x.Uuid == archivePeriodUuid).FromNullable();
        }

        private static Maybe<OperationError> UpdateArchivePeriod(ArchivePeriod period, DateTime startDate, DateTime endDate, string archiveId, bool approved)
        {
            period.UniqueArchiveId = archiveId;
            period.Approved = approved;

            var error = period.UpdatePeriod(startDate, endDate);
            if (error.HasValue)
            {
                return error.Value;
            }

            return Maybe<OperationError>.None;
        }

        public Result<IEnumerable<ItSystemUsagePersonalData>, OperationError> UpdateDataSensitivityLevels(IEnumerable<SensitiveDataLevel> sensitiveDataLevels)
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

            var removedPersonalData = new List<ItSystemUsagePersonalData>();
            if (!SensitiveDataLevelExists(SensitiveDataLevel.PERSONALDATA))
            {
                removedPersonalData.AddRange(ResetPersonalData());
            }

            return removedPersonalData;
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

        public ItSystemUsageValidationResult CheckSystemValidity()
        {
            var errors = new List<ItSystemUsageValidationError>();

            var today = DateTime.UtcNow.Date;

            var dateErrors = CheckDatesValidity(today).ToList();
            var hasLifeCycleStatusValidationError = CheckLifeCycleValidity();
            var hasContractValidityError = CheckContractValidity();

            errors.AddRange(dateErrors);
            if (hasLifeCycleStatusValidationError.HasValue)
            {
                errors.Add(hasLifeCycleStatusValidationError.Value);
            }
            if (hasContractValidityError.HasValue)
            {
                errors.Add(hasContractValidityError.Value);
            }

            return new ItSystemUsageValidationResult(errors);
        }

        public Result<ItSystemUsagePersonalData, OperationError> AddPersonalData(GDPRPersonalDataOption option)
        {
            if (SensitiveDataLevelExists(SensitiveDataLevel.PERSONALDATA) == false)
            {
                return new OperationError($"You cannot add {nameof(PersonalDataOptions)} before adding {nameof(SensitiveDataLevel)}.{nameof(SensitiveDataLevel.PERSONALDATA)}", OperationFailure.BadState);
            }

            if (GetPersonalData(option).HasValue)
            {
                return new OperationError($"An option with value: '{option}' already exists", OperationFailure.Conflict);
            }

            var personalDataOption = new ItSystemUsagePersonalData() { ItSystemUsage = this, PersonalData = option };
            PersonalDataOptions.Add(personalDataOption);
            return personalDataOption;
        }

        public Result<ItSystemUsagePersonalData, OperationError> RemovePersonalData(GDPRPersonalDataOption option)
        {
            if (SensitiveDataLevelExists(SensitiveDataLevel.PERSONALDATA) == false)
            {
                return new OperationError($"You cannot remove {nameof(PersonalDataOptions)} before adding {nameof(SensitiveDataLevel)}.{nameof(SensitiveDataLevel.PERSONALDATA)}", OperationFailure.BadState);
            }

            var personalDataOptionResult = GetPersonalData(option);
            if (personalDataOptionResult.IsNone)
                return new OperationError($"PersonalData: \"{option}\" wasn't found", OperationFailure.NotFound);

            var personalDataOption = personalDataOptionResult.Value;
            PersonalDataOptions.Remove(personalDataOption);
            return personalDataOption;
        }

        private IEnumerable<ItSystemUsagePersonalData> ResetPersonalData()
        {
            var dataBeforeRemoval = PersonalDataOptions.ToList();
            PersonalDataOptions.Clear();

            return dataBeforeRemoval;
        }

        public Maybe<ItSystemUsagePersonalData> GetPersonalData(GDPRPersonalDataOption option)
        {
            return PersonalDataOptions.FirstOrDefault(x => x.PersonalData == option).FromNullable();
        }

        private Maybe<OrganizationUnit> GetOrganizationUnit(Guid uuid)
        {
            return Organization.GetOrganizationUnit(uuid);
        }

        private IEnumerable<OrganizationUnit> GetUsedByOrganizationUnits()
        {
            return UsedBy.Select(x => x.OrganizationUnit).ToList();
        }

        private IEnumerable<ItSystemUsageValidationError> CheckDatesValidity(DateTime todayReference)
        {
            if (Concluded == null && ExpirationDate == null)
                yield break;

            var today = todayReference.Date;
            var startDate = (Concluded ?? today).Date;
            var endDate = (ExpirationDate ?? DateTime.MaxValue).Date;

            //Valid yet?
            if (today < startDate)
            {
                yield return ItSystemUsageValidationError.StartDateNotPassed;
            }

            //Expired?
            if (today > endDate)
            {
                yield return ItSystemUsageValidationError.EndDatePassed;
            }
        }

        private Maybe<ItSystemUsageValidationError> CheckLifeCycleValidity()
        {
            return LifeCycleStatus == LifeCycleStatusType.NotInUse
                ? ItSystemUsageValidationError.NotOperationalAccordingToLifeCycle
                : Maybe<ItSystemUsageValidationError>.None;
        }

        private Maybe<ItSystemUsageValidationError> CheckContractValidity()
        {
            return MainContract?.ItContract?.IsActive == false
                ? ItSystemUsageValidationError.MainContractNotActive
                : Maybe<ItSystemUsageValidationError>.None;
        }
    }
}
