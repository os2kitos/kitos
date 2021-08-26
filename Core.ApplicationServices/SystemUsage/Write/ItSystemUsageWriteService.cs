using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.KLE;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.References;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage.Relations;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Core.DomainServices.Options;
using Core.DomainServices.Role;
using Core.DomainServices.SystemUsage;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Newtonsoft.Json;
using Serilog;

namespace Core.ApplicationServices.SystemUsage.Write
{
    public class ItSystemUsageWriteService : IItSystemUsageWriteService
    {
        private readonly IItSystemUsageService _systemUsageService;
        private readonly ITransactionManager _transactionManager;
        private readonly IItSystemService _systemService;
        private readonly IOrganizationService _organizationService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOptionsService<ItSystemUsage, ItSystemCategories> _systemCategoriesOptionsService;
        private readonly IOptionsService<ItSystemUsage, ArchiveType> _archiveTypeOptionsService;
        private readonly IOptionsService<ItSystemUsage, ArchiveLocation> _archiveLocationOptionsService;
        private readonly IOptionsService<ItSystemUsage, ArchiveTestLocation> _archiveTestLocationOptionsService;
        private readonly IItsystemUsageRelationsService _systemUsageRelationsService;
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IItContractService _contractService;
        private readonly IItProjectService _projectService;
        private readonly IKLEApplicationService _kleApplicationService;
        private readonly IReferenceService _referenceService;
        private readonly IDatabaseControl _databaseControl;
        private readonly IDomainEvents _domainEvents;
        private readonly ILogger _logger;
        private readonly IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage> _roleAssignmentService;
        private readonly IAttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem> _sensitivePersonDataAssignmentService;
        private readonly IAttachedOptionsAssignmentService<RegisterType, ItSystemUsage> _registerTypeAssignmentService;
        private readonly IGenericRepository<ItSystemUsageSensitiveDataLevel> _sensitiveDataLevelRepository;

        public ItSystemUsageWriteService(
            IItSystemUsageService systemUsageService,
            ITransactionManager transactionManager,
            IItSystemService systemService,
            IOrganizationService organizationService,
            IAuthorizationContext authorizationContext,
            IOptionsService<ItSystemUsage, ItSystemCategories> systemCategoriesOptionsService,
            IItContractService contractService,
            IItProjectService projectService,
            IKLEApplicationService kleApplicationService,
            IReferenceService referenceService,
            IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage> roleAssignmentService,
            IAttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem> sensitivePersonDataAssignmentService,
            IAttachedOptionsAssignmentService<RegisterType, ItSystemUsage> registerTypeAssignmentService,
            IGenericRepository<ItSystemUsageSensitiveDataLevel> sensitiveDataLevelRepository,
            IDatabaseControl databaseControl,
            IDomainEvents domainEvents,
            ILogger logger,
            IOptionsService<ItSystemUsage, ArchiveType> archiveTypeOptionsService,
            IOptionsService<ItSystemUsage, ArchiveLocation> archiveLocationOptionsService,
            IOptionsService<ItSystemUsage, ArchiveTestLocation> archiveTestLocationOptionsService,
            IItsystemUsageRelationsService systemUsageRelationsService,
            IEntityIdentityResolver identityResolver)
        {
            _systemUsageService = systemUsageService;
            _transactionManager = transactionManager;
            _systemService = systemService;
            _organizationService = organizationService;
            _authorizationContext = authorizationContext;
            _systemCategoriesOptionsService = systemCategoriesOptionsService;
            _contractService = contractService;
            _projectService = projectService;
            _kleApplicationService = kleApplicationService;
            _referenceService = referenceService;
            _databaseControl = databaseControl;
            _domainEvents = domainEvents;
            _logger = logger;
            _roleAssignmentService = roleAssignmentService;
            _sensitivePersonDataAssignmentService = sensitivePersonDataAssignmentService;
            _registerTypeAssignmentService = registerTypeAssignmentService;
            _sensitiveDataLevelRepository = sensitiveDataLevelRepository;
            _archiveTypeOptionsService = archiveTypeOptionsService;
            _archiveLocationOptionsService = archiveLocationOptionsService;
            _archiveTestLocationOptionsService = archiveTestLocationOptionsService;
            _systemUsageRelationsService = systemUsageRelationsService;
            _identityResolver = identityResolver;
        }

        public Result<ItSystemUsage, OperationError> Create(SystemUsageCreationParameters parameters)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            var systemResult = _systemService.GetSystem(parameters.SystemUuid);
            if (systemResult.Failed)
            {
                _logger.Error("Failed to retrieve itSystem with id {uuid}. Error {error}", parameters.SystemUuid, systemResult.Error.ToString());
                return new OperationError("Unable to resolve IT-System:" + systemResult.Error.Message.GetValueOrEmptyString(), systemResult.Error.FailureType);
            }

            var organizationResult = _organizationService.GetOrganization(parameters.OrganizationUuid);
            if (organizationResult.Failed)
            {
                _logger.Error("Failed to retrieve organization with id {uuid}. Error {error}", parameters.OrganizationUuid, organizationResult.Error.ToString());
                return new OperationError("Unable to resolve IT-System:" + organizationResult.Error.Message.GetValueOrEmptyString(), organizationResult.Error.FailureType);
            }

            var creationResult = _systemUsageService
                .CreateNew(systemResult.Value.Id, organizationResult.Value.Id)
                .Bind(createdSystemUsage => Update(() => createdSystemUsage, parameters.AdditionalValues));

            if (creationResult.Ok)
            {
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return creationResult;
        }

        public Result<ItSystemUsage, OperationError> Update(Guid systemUsageUuid, SystemUsageUpdateParameters parameters)
        {
            return Update(() => _systemUsageService.GetByUuid(systemUsageUuid), parameters);
        }
        private Result<ItSystemUsage, OperationError> Update(Func<Result<ItSystemUsage, OperationError>> getItSystemUsage, SystemUsageUpdateParameters parameters)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

            var result = getItSystemUsage()
                    .Bind(WithWriteAccess)
                    .Bind(systemUsage => PerformUpdates(systemUsage, parameters));

            if (result.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(result.Value));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return result;
        }

        private Result<ItSystemUsage, OperationError> PerformUpdates(ItSystemUsage systemUsage, SystemUsageUpdateParameters parameters)
        {
            //Optionally apply changes across the entire update specification
            return systemUsage.WithOptionalUpdate(parameters.GeneralProperties, PerformGeneralDataPropertiesUpdate)
                    .Bind(usage => usage.WithOptionalUpdate(parameters.Roles, PerformRoleAssignmentUpdates))
                    .Bind(usage => usage.WithOptionalUpdate(parameters.OrganizationalUsage, PerformOrganizationalUsageUpdate))
                    .Bind(usage => usage.WithOptionalUpdate(parameters.KLE, PerformKLEUpdate))
                    .Bind(usage => usage.WithOptionalUpdate(parameters.ExternalReferences, PerformReferencesUpdate))
                    .Bind(usage => usage.WithOptionalUpdate(parameters.GDPR, PerformGDPRUpdates))
                    .Bind(usage => usage.WithOptionalUpdate(parameters.Archiving, PerformArchivingUpdate));
        }

        private Result<ItSystemUsage, OperationError> PerformGDPRUpdates(ItSystemUsage itSystemUsage, UpdatedSystemUsageGDPRProperties parameters)
        {
            //General GDPR registrations
            return itSystemUsage.WithOptionalUpdate(parameters.Purpose, (usage, newPurpose) => usage.GeneralPurpose = newPurpose)
                .Bind(usage => usage.WithOptionalUpdate(parameters.BusinessCritical, (systemUsage, businessCritical) => systemUsage.isBusinessCritical = businessCritical))
                .Bind(usage => usage.WithOptionalUpdate(parameters.HostedAt, (systemUsage, hostedAt) => systemUsage.HostedAt = hostedAt))
                .Bind(usage => usage.WithOptionalUpdate(parameters.DirectoryDocumentation, (systemUsage, newLink) =>
                   {
                       systemUsage.LinkToDirectoryUrlName = newLink.Select(x => x.Name).GetValueOrDefault();
                       systemUsage.LinkToDirectoryUrl = newLink.Select(x => x.Url).GetValueOrDefault();
                   }))

                //Registered data sensitivity
                .Bind(usage => usage.WithOptionalUpdate(parameters.DataSensitivityLevels, (systemUsage, levels) => UpdateSensitivityLevels(levels, systemUsage)))
                .Bind(usage => usage.WithOptionalUpdate(parameters.SensitivePersonDataUuids, UpdateSensitivePersonDataIds))
                .Bind(usage => usage.WithOptionalUpdate(parameters.RegisteredDataCategoryUuids, UpdateRegisteredDataCategories))

                //Technical precautions
                .Bind(usage => usage.WithOptionalUpdate(parameters.TechnicalPrecautionsInPlace, (systemUsage, precautions) => systemUsage.precautions = precautions))
                .Bind(usage => usage.WithOptionalUpdate(parameters.TechnicalPrecautionsApplied, UpdateAppliedTechnicalPrecautions))
                .Bind(usage => usage.WithOptionalUpdate(parameters.TechnicalPrecautionsDocumentation,
                    (systemUsage, newLink) =>
                    {
                        systemUsage.TechnicalSupervisionDocumentationUrlName = newLink.Select(x => x.Name).GetValueOrDefault();
                        systemUsage.TechnicalSupervisionDocumentationUrl = newLink.Select(x => x.Url).GetValueOrDefault();
                    }))

                //User supervision
                .Bind(usage => usage.WithOptionalUpdate(parameters.UserSupervision, (systemUsage, supervision) => systemUsage.UserSupervision = supervision))
                .Bind(usage => usage.WithOptionalUpdate(parameters.UserSupervisionDate, (systemUsage, date) => systemUsage.UserSupervisionDate = date))
                .Bind(usage => usage.WithOptionalUpdate(parameters.UserSupervisionDocumentation, (systemUsage, newLink) =>
                   {
                       systemUsage.UserSupervisionDocumentationUrlName = newLink.Select(x => x.Name).GetValueOrDefault();
                       systemUsage.UserSupervisionDocumentationUrl = newLink.Select(x => x.Url).GetValueOrDefault();
                   }))

                //Risk assessments
                .Bind(usage => usage.WithOptionalUpdate(parameters.RiskAssessmentConducted, (systemUsage, conducted) => systemUsage.riskAssessment = conducted))
                .Bind(usage => usage.WithOptionalUpdate(parameters.RiskAssessmentConductedDate, (systemUsage, date) => systemUsage.riskAssesmentDate = date))
                .Bind(usage => usage.WithOptionalUpdate(parameters.RiskAssessmentResult, (systemUsage, result) => systemUsage.preriskAssessment = result))
                .Bind(usage => usage.WithOptionalUpdate(parameters.RiskAssessmentDocumentation, (systemUsage, newLink) =>
                   {
                       systemUsage.RiskSupervisionDocumentationUrlName = newLink.Select(x => x.Name).GetValueOrDefault();
                       systemUsage.RiskSupervisionDocumentationUrl = newLink.Select(x => x.Url).GetValueOrDefault();
                   }))
                .Bind(usage => usage.WithOptionalUpdate(parameters.RiskAssessmentNotes, (systemUsage, notes) => systemUsage.noteRisks = notes))

                //DPIA
                .Bind(usage => usage.WithOptionalUpdate(parameters.DPIAConducted, (systemUsage, conducted) => systemUsage.DPIA = conducted))
                .Bind(usage => usage.WithOptionalUpdate(parameters.DPIADate, (systemUsage, date) => systemUsage.DPIADateFor = date))
                .Bind(usage => usage.WithOptionalUpdate(parameters.DPIADocumentation, (systemUsage, newLink) =>
                   {
                       systemUsage.DPIASupervisionDocumentationUrlName = newLink.Select(x => x.Name).GetValueOrDefault();
                       systemUsage.DPIASupervisionDocumentationUrl = newLink.Select(x => x.Url).GetValueOrDefault();
                   }))

                //Data retention
                .Bind(usage => usage.WithOptionalUpdate(parameters.RetentionPeriodDefined, (systemUsage, retentionPeriodDefined) => systemUsage.answeringDataDPIA = retentionPeriodDefined))
                .Bind(usage => usage.WithOptionalUpdate(parameters.NextDataRetentionEvaluationDate, (systemUsage, date) => systemUsage.DPIAdeleteDate = date))
                .Bind(usage => usage.WithOptionalUpdate(parameters.DataRetentionEvaluationFrequencyInMonths, (systemUsage, frequencyInMonths) => systemUsage.numberDPIA = frequencyInMonths.GetValueOrDefault()));
        }

        private Maybe<OperationError> UpdateSensitivityLevels(Maybe<IEnumerable<SensitiveDataLevel>> levels, ItSystemUsage systemUsage)
        {
            var newLevels = levels.GetValueOrFallback(new List<SensitiveDataLevel>()).ToList();
            var levelsBefore = systemUsage.SensitiveDataLevels.ToList();
            var error = systemUsage.UpdateDataSensitivityLevels(newLevels);
            if (error.HasValue)
                return error;

            var levelsRemoved = levelsBefore.Except(systemUsage.SensitiveDataLevels.ToList()).ToList();

            foreach (var removedSensitiveDataLevel in levelsRemoved)
            {
                _sensitiveDataLevelRepository.Delete(removedSensitiveDataLevel);
            }

            return Maybe<OperationError>.None;
        }

        private static Maybe<OperationError> UpdateAppliedTechnicalPrecautions(ItSystemUsage systemUsage, Maybe<IEnumerable<TechnicalPrecaution>> newPrecautions)
        {
            return systemUsage.UpdateTechnicalPrecautions(newPrecautions.GetValueOrFallback(new List<TechnicalPrecaution>()));
        }

        private Maybe<OperationError> UpdateRegisteredDataCategories(ItSystemUsage systemUsage, Maybe<IEnumerable<Guid>> registerTypeUuids)
        {
            return _registerTypeAssignmentService
                .UpdateAssignedOptions(systemUsage, registerTypeUuids.GetValueOrFallback(new List<Guid>()))
                .Match(_ => Maybe<OperationError>.None, error => error);
        }

        private Maybe<OperationError> UpdateSensitivePersonDataIds(ItSystemUsage systemUsage, Maybe<IEnumerable<Guid>> sensitiveDataTypeUuids)
        {
            return _sensitivePersonDataAssignmentService
                .UpdateAssignedOptions(systemUsage, sensitiveDataTypeUuids.GetValueOrFallback(new List<Guid>()))
                .Match(_ => Maybe<OperationError>.None, error => error);
        }

        private Result<ItSystemUsage, OperationError> PerformReferencesUpdate(ItSystemUsage systemUsage, IEnumerable<UpdatedExternalReferenceProperties> externalReferences)
        {
            //Clear existing state
            systemUsage.ClearMasterReference();
            _referenceService.DeleteBySystemUsageId(systemUsage.Id);
            var newReferences = externalReferences.ToList();
            if (newReferences.Any())
            {
                var masterReferencesCount = newReferences.Count(x => x.MasterReference);

                switch (masterReferencesCount)
                {
                    case < 1:
                        return new OperationError("A master reference must be defined", OperationFailure.BadInput);
                    case > 1:
                        return new OperationError("Only one reference can be master reference", OperationFailure.BadInput);
                }

                foreach (var referenceProperties in newReferences)
                {
                    var result = _referenceService.AddReference(systemUsage.Id, ReferenceRootType.SystemUsage, referenceProperties.Title, referenceProperties.DocumentId, referenceProperties.Url);

                    if (result.Failed)
                        return new OperationError($"Failed to add reference with data:{JsonConvert.SerializeObject(referenceProperties)}. Error:{result.Error.Message.GetValueOrEmptyString()}", result.Error.FailureType);

                    if (referenceProperties.MasterReference)
                    {
                        var masterReferenceResult = systemUsage.SetMasterReference(result.Value);
                        if (masterReferenceResult.Failed)
                            return new OperationError($"Failed while setting the master reference:{masterReferenceResult.Error.Message.GetValueOrEmptyString()}", masterReferenceResult.Error.FailureType);
                    }
                }
            }

            return systemUsage;
        }

        private Result<ItSystemUsage, OperationError> PerformArchivingUpdate(ItSystemUsage itSystemUsage, UpdatedSystemUsageArchivingParameters archivingProperties)
        {
            return itSystemUsage.WithOptionalUpdate(archivingProperties.ArchiveDuty, (usage, archiveDuty) => usage.ArchiveDuty = archiveDuty)
                .Bind(systemUsage => systemUsage.WithOptionalUpdate(archivingProperties.ArchiveTypeUuid, UpdateArchiveType))
                .Bind(systemUsage => systemUsage.WithOptionalUpdate(archivingProperties.ArchiveLocationUuid, UpdateArchiveLocation))
                .Bind(systemUsage => systemUsage.WithOptionalUpdate(archivingProperties.ArchiveTestLocationUuid, UpdateArchiveTestLocation))
                .Bind(systemUsage => systemUsage.WithOptionalUpdate(archivingProperties.ArchiveSupplierOrganizationUuid, UpdateArchiveSupplierOrganization))
                .Bind(systemUsage => systemUsage.WithOptionalUpdate(archivingProperties.ArchiveActive, (usage, archiveActive) => usage.ArchiveFromSystem = archiveActive))
                .Bind(systemUsage => systemUsage.WithOptionalUpdate(archivingProperties.ArchiveNotes, (usage, archiveNotes) => usage.ArchiveNotes = archiveNotes))
                .Bind(systemUsage => systemUsage.WithOptionalUpdate(archivingProperties.ArchiveFrequencyInMonths, (usage, archiveFrequencyInMonths) => usage.ArchiveFreq = archiveFrequencyInMonths))
                .Bind(systemUsage => systemUsage.WithOptionalUpdate(archivingProperties.ArchiveDocumentBearing, (usage, archiveDocumentBearing) => usage.Registertype = archiveDocumentBearing))
                .Bind(systemUsage => systemUsage.WithOptionalUpdate(archivingProperties.ArchiveJournalPeriods, UpdateArchiveJournalPeriods));
        }

        private Maybe<OperationError> UpdateArchiveJournalPeriods(ItSystemUsage systemUsage, Maybe<IEnumerable<SystemUsageJournalPeriod>> journalPeriods)
        {
            //Clear existing values
            var removeResult = _systemUsageService.RemoveAllArchivePeriods(systemUsage.Id);
            if (removeResult.Failed)
                return new OperationError($"Failed to remove all ArchiveJournalPeriods as part of the update. Remove error: {removeResult.Error.Message.GetValueOrEmptyString()}", removeResult.Error.FailureType);

            if (journalPeriods.IsNone)
            {
                // No new journal periods
                return Maybe<OperationError>.None;
            }

            var newPeriods = journalPeriods.Value.ToList();
            foreach (var newPeriod in newPeriods)
            {
                var addResult = _systemUsageService.AddArchivePeriod(systemUsage.Id, newPeriod.StartDate,
                    newPeriod.EndDate, newPeriod.ArchiveId, newPeriod.Approved);

                if (addResult.Failed)
                    return new OperationError(
                        $"Failed to add ArchiveJournalPeriod as part of the update. Add error: {addResult.Error.Message.GetValueOrEmptyString()}",
                        addResult.Error.FailureType);
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> UpdateArchiveSupplierOrganization(ItSystemUsage systemUsage, Maybe<Guid> archiveSupplierOrganization)
        {
            if (archiveSupplierOrganization.IsNone)
            {
                systemUsage.ResetArchiveSupplierOrganization();
                return Maybe<OperationError>.None;
            }

            var orgByUuid = _organizationService.GetOrganization(archiveSupplierOrganization.Value);

            if (orgByUuid.Failed)
                return new OperationError($"Failed to get organization for ArchiveSupplierOrganization. Original error message: {orgByUuid.Error.Message.GetValueOrEmptyString()}", orgByUuid.Error.FailureType);

            return systemUsage.UpdateArchiveSupplierOrganization(orgByUuid.Value);
        }

        private Maybe<OperationError> UpdateArchiveTestLocation(ItSystemUsage systemUsage, Maybe<Guid> archiveTestLocation)
        {
            if (archiveTestLocation.IsNone)
            {
                systemUsage.ResetArchiveTestLocation();
                return Maybe<OperationError>.None;
            }

            var optionByUuid = _archiveTestLocationOptionsService.GetOptionByUuid(systemUsage.OrganizationId, archiveTestLocation.Value);

            if (optionByUuid.IsNone)
                return new OperationError("Invalid ArchiveTestLocation Uuid", OperationFailure.BadInput);

            //Not a change from current state so do not apply availability constraint
            if (systemUsage.ArchiveTestLocationId != null && systemUsage.ArchiveTestLocationId == optionByUuid.Value.option.Id)
                return Maybe<OperationError>.None;

            if (!optionByUuid.Value.available)
                return new OperationError("ArchiveTestLocation is not available in the organization.", OperationFailure.BadInput);

            return systemUsage.UpdateArchiveTestLocation(optionByUuid.Value.option);
        }

        private Maybe<OperationError> UpdateArchiveLocation(ItSystemUsage systemUsage, Maybe<Guid> archiveLocation)
        {
            if (archiveLocation.IsNone)
            {
                systemUsage.ResetArchiveLocation();
                return Maybe<OperationError>.None;
            }

            var optionByUuid = _archiveLocationOptionsService.GetOptionByUuid(systemUsage.OrganizationId, archiveLocation.Value);

            if (optionByUuid.IsNone)
                return new OperationError("Invalid ArchiveLocation Uuid", OperationFailure.BadInput);

            //Not a change from current state so do not apply availability constraint
            if (systemUsage.ArchiveLocationId != null && systemUsage.ArchiveLocationId == optionByUuid.Value.option.Id)
                return Maybe<OperationError>.None;

            if (!optionByUuid.Value.available)
                return new OperationError("ArchiveLocation is not available in the organization.", OperationFailure.BadInput);

            return systemUsage.UpdateArchiveLocation(optionByUuid.Value.option);
        }

        private Maybe<OperationError> UpdateArchiveType(ItSystemUsage systemUsage, Maybe<Guid> archiveType)
        {
            if (archiveType.IsNone)
            {
                systemUsage.ResetArchiveType();
                return Maybe<OperationError>.None;
            }

            var optionByUuid = _archiveTypeOptionsService.GetOptionByUuid(systemUsage.OrganizationId, archiveType.Value);

            if (optionByUuid.IsNone)
                return new OperationError("Invalid ArchiveType Uuid", OperationFailure.BadInput);

            //Not a change from current state so do not apply availability constraint
            if (systemUsage.ArchiveTypeId != null && systemUsage.ArchiveTypeId == optionByUuid.Value.option.Id)
                return Maybe<OperationError>.None;

            if (!optionByUuid.Value.available)
                return new OperationError("ArchiveType is not available in the organization.", OperationFailure.BadInput);

            return systemUsage.UpdateArchiveType(optionByUuid.Value.option);
        }

        private Result<ItSystemUsage, OperationError> PerformKLEUpdate(ItSystemUsage systemUsage, UpdatedSystemUsageKLEDeviationParameters changes)
        {
            if (changes.AddedKLEUuids.HasChange || changes.RemovedKLEUuids.HasChange)
            {
                var addedTaskRefs = changes.AddedKLEUuids.MapOptionalChangeWithFallback(systemUsage.TaskRefs.Select(x => x.Uuid).ToList()).GetValueOrFallback(Enumerable.Empty<Guid>());
                var removedTaskRefs = changes.RemovedKLEUuids.MapOptionalChangeWithFallback(systemUsage.TaskRefsOptOut.Select(x => x.Uuid).ToList()).GetValueOrFallback(Enumerable.Empty<Guid>());

                var additions = new List<TaskRef>();
                foreach (var uuid in addedTaskRefs)
                {
                    var result = _kleApplicationService.GetKle(uuid);
                    if (result.Failed)
                    {
                        return new OperationError($"Failed to load KLE with uuid:{uuid}:{result.Error.Message.GetValueOrEmptyString()}", result.Error.FailureType);
                    }
                    additions.Add(result.Value.kle);
                }

                var removals = new List<TaskRef>();
                foreach (var uuid in removedTaskRefs)
                {
                    var result = _kleApplicationService.GetKle(uuid);
                    if (result.Failed)
                    {
                        return new OperationError($"Failed to load KLE with uuid:{uuid}:{result.Error.Message.GetValueOrEmptyString()}", result.Error.FailureType);
                    }
                    removals.Add(result.Value.kle);
                }

                return systemUsage.UpdateKLEDeviations(additions, removals).Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage);
            }

            return systemUsage;
        }

        private Result<ItSystemUsage, OperationError> PerformOrganizationalUsageUpdate(ItSystemUsage systemUsage, UpdatedSystemUsageOrganizationalUseParameters updatedParameters)
        {
            if (updatedParameters.ResponsibleOrganizationUnitUuid.HasChange ||
                updatedParameters.UsingOrganizationUnitUuids.HasChange)
            {
                var nextResponsibleOrgUuid = updatedParameters.ResponsibleOrganizationUnitUuid.MapOptionalChangeWithFallback(systemUsage.ResponsibleUsage.FromNullable().Select(x => x.OrganizationUnit.Uuid));
                var nextResponsibleOrg = Maybe<OrganizationUnit>.None;
                if (nextResponsibleOrgUuid.HasValue)
                {
                    var organizationUnitResult = _organizationService.GetOrganizationUnit(nextResponsibleOrgUuid.Value);
                    if (organizationUnitResult.Failed)
                        return new OperationError($"Failed to fetch responsible org unit: {organizationUnitResult.Error.Message.GetValueOrEmptyString()}", organizationUnitResult.Error.FailureType);
                    nextResponsibleOrg = organizationUnitResult.Value;
                }
                var usingOrganizationUnits = updatedParameters.UsingOrganizationUnitUuids.MapOptionalChangeWithFallback(systemUsage.UsedBy.Select(x => x.OrganizationUnit.Uuid).ToList().FromNullable<IEnumerable<Guid>>());
                var nextUsingOrganizationUnits = new List<OrganizationUnit>();
                if (usingOrganizationUnits.HasValue)
                {
                    var usingOrgUnitUuids = usingOrganizationUnits.Value.ToList();
                    if (usingOrgUnitUuids.Any())
                    {
                        foreach (var usingOrgUnitUuid in usingOrgUnitUuids)
                        {
                            var result = _organizationService.GetOrganizationUnit(usingOrgUnitUuid);
                            if (result.Failed)
                                return new OperationError($"Failed to using org unit with id {usingOrgUnitUuid}: {result.Error.Message.GetValueOrEmptyString()}", result.Error.FailureType);
                            nextUsingOrganizationUnits.Add(result.Value);
                        }
                    }
                }

                return systemUsage
                    .UpdateOrganizationalUsage(nextUsingOrganizationUnits, nextResponsibleOrg)
                    .Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage);
            }
            //No changes provided - skip
            return systemUsage;
        }

        private Result<ItSystemUsage, OperationError> PerformGeneralDataPropertiesUpdate(ItSystemUsage itSystemUsage, UpdatedSystemUsageGeneralProperties generalProperties)
        {
            return itSystemUsage.WithOptionalUpdate(generalProperties.LocalCallName, (systemUsage, localCallName) => systemUsage.UpdateLocalCallName(localCallName))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.LocalSystemId, (systemUsage, localSystemId) => systemUsage.UpdateLocalSystemId(localSystemId)))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.DataClassificationUuid, UpdateDataClassification))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.Notes, (systemUsage, notes) => systemUsage.Note = notes))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.SystemVersion, (systemUsage, version) => systemUsage.UpdateSystemVersion(version)))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.NumberOfExpectedUsersInterval, UpdateExpectedUsersInterval))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.EnforceActive, (systemUsage, enforceActive) => systemUsage.Active = enforceActive.GetValueOrFallback(false)))
                .Bind(usage => UpdateValidityPeriod(usage, generalProperties))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.MainContractUuid, UpdateMainContract))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.AssociatedProjectUuids, UpdateProjectAssociations));
        }

        private static Result<ItSystemUsage, OperationError> UpdateValidityPeriod(ItSystemUsage usage, UpdatedSystemUsageGeneralProperties generalProperties)
        {
            if (generalProperties.ValidFrom.IsUnchanged && generalProperties.ValidTo.IsUnchanged)
                return usage; //Not changes provided

            var newValidFrom = generalProperties.ValidFrom.MapDateTimeOptionalChangeWithFallback(usage.Concluded);
            var newValidTo = generalProperties.ValidTo.MapDateTimeOptionalChangeWithFallback(usage.ExpirationDate);

            return usage.UpdateSystemValidityPeriod(newValidFrom, newValidTo).Match<Result<ItSystemUsage, OperationError>>(error => error, () => usage);
        }

        private Result<ItSystemUsage, OperationError> UpdateProjectAssociations(ItSystemUsage systemUsage, Maybe<IEnumerable<Guid>> projectUuids)
        {
            if (projectUuids.IsNone)
            {
                systemUsage.ResetProjectAssociations();
                return systemUsage;
            }

            var itProjects = new List<ItProject>();
            foreach (var uuid in projectUuids.Value)
            {
                var result = _projectService.GetProject(uuid);

                if (result.Failed)
                    return new OperationError($"Error loading project with id: {uuid}. Error:{result.Error.Message.GetValueOrEmptyString()}", result.Error.FailureType);

                itProjects.Add(result.Value);
            }

            return systemUsage.SetProjectAssociations(itProjects).Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage);
        }

        private Result<ItSystemUsage, OperationError> UpdateMainContract(ItSystemUsage systemUsage, Maybe<Guid> contractId)
        {
            if (contractId.IsNone)
            {
                systemUsage.ResetMainContract();
                return systemUsage;
            }

            var contractResult = _contractService.GetContract(contractId.Value);
            if (contractResult.Failed)
                return new OperationError($"Failure getting the contract:{contractResult.Error.Message.GetValueOrEmptyString()}", contractResult.Error.FailureType);

            return systemUsage.SetMainContract(contractResult.Value).Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage);
        }

        private static Result<ItSystemUsage, OperationError> UpdateExpectedUsersInterval(ItSystemUsage systemUsage, Maybe<(int lower, int? upperBound)> newInterval)
        {
            if (newInterval.IsNone)
            {
                systemUsage.ResetUserCount();
                return systemUsage;
            }

            return systemUsage.SetExpectedUsersInterval(newInterval.Value).Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage);
        }

        private Maybe<OperationError> UpdateDataClassification(ItSystemUsage systemUsage, Maybe<Guid> dataClassificationOptionId)
        {
            if (dataClassificationOptionId.IsNone)
            {
                systemUsage.ResetSystemCategories();
                return Maybe<OperationError>.None;
            }

            var optionByUuid = _systemCategoriesOptionsService.GetOptionByUuid(systemUsage.OrganizationId, dataClassificationOptionId.Value);

            if (optionByUuid.IsNone)
                return new OperationError("Invalid DataClassification Uuid", OperationFailure.BadInput);

            //Not a change from current state so do not apply availability constraint
            if (systemUsage.ItSystemCategoriesId != null && systemUsage.ItSystemCategoriesId == optionByUuid.Value.option.Id)
                return Maybe<OperationError>.None;

            if (!optionByUuid.Value.available)
                return new OperationError("DataClassification is not available in the organization.", OperationFailure.BadInput);

            return systemUsage.UpdateSystemCategories(optionByUuid.Value.option);
        }

        private Result<ItSystemUsage, OperationError> PerformRoleAssignmentUpdates(ItSystemUsage itSystemUsage, UpdatedSystemUsageRoles usageRoles)
        {
            return itSystemUsage.WithOptionalUpdate(usageRoles.UserRolePairs, UpdateRoles);
        }

        private Result<ItSystemUsage, OperationError> UpdateRoles(ItSystemUsage systemUsage, Maybe<IEnumerable<UserRolePair>> userRolePairs)
        {
            // Compare lists to find which needs to be remove and which need to be added
            var rightsKeys = systemUsage.Rights.Select(x => new UserRolePair { RoleUuid = x.Role.Uuid, UserUuid = x.User.Uuid }).ToList();
            var userRoleKeys = userRolePairs.GetValueOrFallback(new List<UserRolePair>()).ToList();

            var toRemove = rightsKeys.Except(userRoleKeys);
            var toAdd = userRoleKeys.Except(rightsKeys);

            foreach (var userRolePair in toRemove)
            {
                var removeResult = _roleAssignmentService.RemoveRole(systemUsage, userRolePair.RoleUuid, userRolePair.UserUuid);

                if (removeResult.Failed)
                    return new OperationError($"Failed to remove role with Uuid: {userRolePair.RoleUuid} from user with Uuid: {userRolePair.UserUuid}, with following error message: {removeResult.Error.Message.GetValueOrEmptyString()}", removeResult.Error.FailureType);
            }

            foreach (var userRolePair in toAdd)
            {
                var assignmentResult = _roleAssignmentService.AssignRole(systemUsage, userRolePair.RoleUuid, userRolePair.UserUuid);

                if (assignmentResult.Failed)
                    return new OperationError($"Failed to assign role with Uuid: {userRolePair.RoleUuid} from user with Uuid: {userRolePair.UserUuid}, with following error message: {assignmentResult.Error.Message.GetValueOrEmptyString()}", assignmentResult.Error.FailureType);
            }

            return systemUsage;
        }

        private Result<ItSystemUsage, OperationError> WithWriteAccess(ItSystemUsage systemUsage)
        {
            return _authorizationContext.AllowModify(systemUsage) ? systemUsage : new OperationError(OperationFailure.Forbidden);
        }

        public Maybe<OperationError> Delete(Guid itSystemUsageUuid)
        {
            return _systemUsageService.GetByUuid(itSystemUsageUuid)
                .Bind(usage => _systemUsageService.Delete(usage.Id))
                .Match(_ => Maybe<OperationError>.None, error => new OperationError($"Failed to delete it system usage with Uuid: {itSystemUsageUuid}, Error message: {error.Message.GetValueOrEmptyString()}", error.FailureType));
        }

        public Result<SystemRelation, OperationError> CreateSystemRelation(Guid fromSystemUsageUuid, SystemRelationParameters parameters)
        {
            return _systemUsageService.GetByUuid(fromSystemUsageUuid)
                .Bind(usage => ResolveRelationParameterIdentities(parameters).Select(ids => (usage, ids)))
                .Bind(usageAndIds =>
                    _systemUsageRelationsService.AddRelation
                        (
                            usageAndIds.usage.Id,
                            usageAndIds.ids.systemUsageId,
                            usageAndIds.ids.interfaceId.Match<int?>(id => id, () => null),
                            parameters.Description,
                            parameters.UrlReference,
                            usageAndIds.ids.frequencyId.Match<int?>(id => id, () => null),
                            usageAndIds.ids.contractId.Match<int?>(id => id, () => null)
                        )
                );
        }

        private Result<(Maybe<int> contractId, Maybe<int> interfaceId, Maybe<int> frequencyId, int systemUsageId), OperationError> ResolveRelationParameterIdentities(SystemRelationParameters parameters)
        {
            return ResolveOptionalId<ItContract>(parameters.AssociatedContractUuid)
                .Bind(id => ResolveOptionalId<ItInterface>(parameters.UsingInterfaceUuid).Select(interfaceId => (contractId: id, interfaceId)))
                .Bind(ids => ResolveOptionalId<RelationFrequencyType>(parameters.RelationFrequencyUuid).Select(id => (ids.contractId, ids.interfaceId, frequencyId: id)))
                .Bind(ids => ResolveRequiredId<ItSystemUsage>(parameters.ToSystemUsageUuid).Select(id => (ids.contractId, ids.interfaceId, ids.contractId, systemUsageId: id)));
        }

        public Result<SystemRelation, OperationError> UpdateSystemRelation(Guid fromSystemUsageUuid, Guid relationUuid, SystemRelationParameters parameters)
        {
            return _systemUsageService.GetByUuid(fromSystemUsageUuid)
                .Bind(usage => ResolveRelationParameterIdentities(parameters).Select(ids => (usage, ids)))
                .Bind(usageAndIds => ResolveRequiredId<SystemRelation>(relationUuid).Select(relationId => (usageAndIds.usage, relationId, usageAndIds.ids)))
                .Bind(usageAndIds =>
                    _systemUsageRelationsService.ModifyRelation(
                        usageAndIds.usage.Id,
                        usageAndIds.relationId,
                        usageAndIds.ids.systemUsageId,
                        parameters.Description,
                        parameters.UrlReference,
                        usageAndIds.ids.interfaceId.Match<int?>(id => id, () => null),
                        usageAndIds.ids.contractId.Match<int?>(id => id, () => null),
                        usageAndIds.ids.frequencyId.Match<int?>(id => id, () => null)
                    )
                );
        }

        public Maybe<OperationError> DeleteSystemRelation(Guid itSystemUsageUuid, Guid itSystemUsageRelationUuid)
        {
            return _systemUsageService
                .GetByUuid(itSystemUsageUuid)
                .Bind(WithWriteAccess)
                .Bind<(int usageId, int relationId)>(usage =>
                {
                    var usageRelation = _identityResolver.ResolveDbId<SystemRelation>(itSystemUsageRelationUuid);
                    if (usageRelation.IsNone)
                        return new OperationError(
                            $"Relation with id:{itSystemUsageRelationUuid} does not exist", OperationFailure.BadInput);
                    return (usage.Id, usageRelation.Value);
                })
                .Bind(usageAndRelation => _systemUsageRelationsService.RemoveRelation(usageAndRelation.usageId, usageAndRelation.relationId))
                .Match(_ => Maybe<OperationError>.None, error => error);
        }

        private Result<int, OperationError> ResolveRequiredId<T>(Guid requiredId) where T : class, IHasUuid, IHasId
        {
            return ResolveOptionalId<T>(requiredId)
                .Bind(result => result.Match<Result<int, OperationError>>(optionalId => optionalId, () => new OperationError(OperationFailure.BadInput)));
        }

        private Result<Maybe<int>, OperationError> ResolveOptionalId<T>(Guid? optionalId) where T : class, IHasUuid, IHasId
        {
            if (optionalId.HasValue)
            {
                var id = _identityResolver.ResolveDbId<T>(optionalId.Value);

                return id.Match<Result<Maybe<int>, OperationError>>
                (
                    dbId => Maybe<int>.Some(dbId),
                    () => new OperationError($"Invalid {typeof(T).Name} Id", OperationFailure.BadInput)
                );
            }

            return Maybe<int>.None;
        }
    }
}
