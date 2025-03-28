using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.KLE;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.References;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage.Relations;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Core.DomainServices.Options;
using Core.DomainServices.Role;
using Core.DomainServices.SystemUsage;
using Infrastructure.Services.DataAccess;
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
        private readonly IKLEApplicationService _kleApplicationService;
        private readonly IReferenceService _referenceService;
        private readonly IDatabaseControl _databaseControl;
        private readonly IDomainEvents _domainEvents;
        private readonly ILogger _logger;
        private readonly IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage> _roleAssignmentService;
        private readonly IAttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem> _sensitivePersonDataAssignmentService;
        private readonly IAttachedOptionsAssignmentService<RegisterType, ItSystemUsage> _registerTypeAssignmentService;
        private readonly IGenericRepository<ItSystemUsageSensitiveDataLevel> _sensitiveDataLevelRepository;
        private readonly IGenericRepository<ItSystemUsagePersonalData> _personalDataOptionsRepository;

        public ItSystemUsageWriteService(
            IItSystemUsageService systemUsageService,
            ITransactionManager transactionManager,
            IItSystemService systemService,
            IOrganizationService organizationService,
            IAuthorizationContext authorizationContext,
            IOptionsService<ItSystemUsage, ItSystemCategories> systemCategoriesOptionsService,
            IItContractService contractService,
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
            IEntityIdentityResolver identityResolver,
            IGenericRepository<ItSystemUsagePersonalData> personalDataOptionsRepository)
        {
            _systemUsageService = systemUsageService;
            _transactionManager = transactionManager;
            _systemService = systemService;
            _organizationService = organizationService;
            _authorizationContext = authorizationContext;
            _systemCategoriesOptionsService = systemCategoriesOptionsService;
            _contractService = contractService;
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
            _personalDataOptionsRepository = personalDataOptionsRepository;
        }

        public Result<ItSystemUsage, OperationError> Create(SystemUsageCreationParameters parameters)
        {
            using var transaction = _transactionManager.Begin();
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

        public Result<ExternalReference, OperationError> AddExternalReference(Guid usageUuid, ExternalReferenceProperties externalReferenceProperties)
        {
            return GetUsageAndAuthorizeWriteAccess(usageUuid)
                .Bind(usage => _referenceService.AddReference(usage.Id, ReferenceRootType.SystemUsage, externalReferenceProperties));
        }

        public Result<ExternalReference, OperationError> UpdateExternalReference(Guid usageUuid, Guid externalReferenceUuid, ExternalReferenceProperties externalReferenceProperties)
        {
            return GetUsageAndAuthorizeWriteAccess(usageUuid)
                .Bind(usage => _referenceService.UpdateReference(usage.Id, ReferenceRootType.SystemUsage, externalReferenceUuid, externalReferenceProperties));
        }

        public Result<ExternalReference, OperationError> DeleteExternalReference(Guid usageUuid, Guid externalReferenceUuid)
        {
            return GetUsageAndAuthorizeWriteAccess(usageUuid)
                .Bind(_ =>
                    {
                        var getIdResult = _identityResolver.ResolveDbId<ExternalReference>(externalReferenceUuid);
                        if (getIdResult.IsNone)
                            return new OperationError($"ExternalReference with uuid: {externalReferenceUuid} was not found", OperationFailure.NotFound);
                        var externalReferenceId = getIdResult.Value;

                        return _referenceService.DeleteByReferenceId(externalReferenceId)
                            .Match(Result<ExternalReference, OperationError>.Success,
                                operationFailure =>
                                    new OperationError($"Failed to remove the ExternalReference with uuid: {externalReferenceUuid}", operationFailure));
                    });
        }

        public Result<ItSystemUsage, OperationError> Update(Guid systemUsageUuid, SystemUsageUpdateParameters parameters)
        {
            return Update(() => _systemUsageService.GetReadableItSystemUsageByUuid(systemUsageUuid), parameters);
        }

        public Result<ItSystemUsage, OperationError> AddRole(Guid systemUsageUuid, UserRolePair assignment)
        {
            return _systemUsageService
                .GetReadableItSystemUsageByUuid(systemUsageUuid)
                .Select(ExtractAssignedRoles)
                .Bind<SystemUsageUpdateParameters>(existingRoles =>
                {
                    if (existingRoles.Contains(assignment))
                    {
                        return new OperationError("Role assignment exists", OperationFailure.Conflict);
                    }
                    return CreateRoleAssignmentUpdate(existingRoles.Append(assignment));
                })
                .Bind(update => Update(systemUsageUuid, update));
        }

        public Result<ItSystemUsage, OperationError> RemoveRole(Guid systemUsageUuid, UserRolePair assignment)
        {
            return _systemUsageService
                .GetReadableItSystemUsageByUuid(systemUsageUuid)
                .Select(ExtractAssignedRoles)
                .Bind<SystemUsageUpdateParameters>(existingRoles =>
                {
                    if (!existingRoles.Contains(assignment))
                    {
                        return new OperationError("Assignment does not exist", OperationFailure.BadInput);
                    }
                    return CreateRoleAssignmentUpdate(existingRoles.Except(assignment.WrapAsEnumerable()));
                })
                .Bind(update => Update(systemUsageUuid, update));
        }

        private Result<ItSystemUsage, OperationError> Update(Func<Result<ItSystemUsage, OperationError>> getItSystemUsage, SystemUsageUpdateParameters parameters)
        {
            using var transaction = _transactionManager.Begin();

            var result = getItSystemUsage()
                    .Bind(WithWriteAccess)
                    .Bind(systemUsage => PerformUpdates(systemUsage, parameters));

            if (result.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(result.Value));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
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
                .Bind(usage => usage.WithOptionalUpdate(parameters.PersonalDataOptions, UpdatePersonalDataOptions))
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
                .Bind(usage => usage.WithOptionalUpdate(parameters.PlannedRiskAssessmentDate, (systemUsage, date) => systemUsage.PlannedRiskAssessmentDate = date))

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
            var result = systemUsage.UpdateDataSensitivityLevels(newLevels);
            if (result.Failed)
                return result.Error;

            var levelsRemoved = levelsBefore.Except(systemUsage.SensitiveDataLevels.ToList()).ToList();

            foreach (var removedSensitiveDataLevel in levelsRemoved)
            {
                _sensitiveDataLevelRepository.Delete(removedSensitiveDataLevel);
            }

            var removedPersonalDataOptions = result.Value;
            _personalDataOptionsRepository.RemoveRange(removedPersonalDataOptions);

            if (!systemUsage.SensitiveDataLevelExists(SensitiveDataLevel.SENSITIVEDATA))
            {
                UpdateSensitivePersonDataIds(systemUsage, new List<Guid>());
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
                .MatchFailure();
        }

        private Maybe<OperationError> UpdatePersonalDataOptions(ItSystemUsage systemUsage, Maybe<IEnumerable<GDPRPersonalDataOption>> personalDataOptions)
        {
            var newOptions = personalDataOptions.GetValueOrFallback(new List<GDPRPersonalDataOption>()).ToList();
            var dataBefore = systemUsage.PersonalDataOptions.Select(x => x.PersonalData).ToList();

            var deltaResult = dataBefore.ComputeDelta(newOptions, x => x).ToList();

            foreach (var (delta, item) in deltaResult)
            {
                switch (delta)
                {
                    case EnumerableExtensions.EnumerableDelta.Added:
                        var additionError = _systemUsageService.AddPersonalDataOption(systemUsage.Id, item);
                        if (additionError.HasValue)
                        {
                            var error = additionError.Value;
                            _logger.Error("Failed removing personData {personDataType} from system usage ({uuid}) Error:{errorCode}: {errorMessage}", item, systemUsage.Uuid, error.FailureType, error.Message.GetValueOrFallback(string.Empty));
                            return additionError;
                        }
                        break;
                    case EnumerableExtensions.EnumerableDelta.Removed:
                        var deletionError = _systemUsageService.RemovePersonalDataOption(systemUsage.Id, item);
                        if (deletionError.HasValue)
                        {
                            var error = deletionError.Value;
                            _logger.Error("Failed adding personData {personDataType} to system usage ({uuid}) Error:{errorCode}: {errorMessage}", item, systemUsage.Uuid, error.FailureType, error.Message.GetValueOrFallback(string.Empty));
                            return deletionError.Value;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(EnumerableExtensions.EnumerableDelta), delta, "Unknown delta type");
                }
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> UpdateSensitivePersonDataIds(ItSystemUsage systemUsage, Maybe<IEnumerable<Guid>> sensitiveDataTypeUuids)
        {
            return _sensitivePersonDataAssignmentService
                .UpdateAssignedOptions(systemUsage, sensitiveDataTypeUuids.GetValueOrFallback(new List<Guid>()))
                .MatchFailure();
        }

        private Result<ItSystemUsage, OperationError> PerformReferencesUpdate(ItSystemUsage systemUsage, IEnumerable<UpdatedExternalReferenceProperties> externalReferences)
        {
            //Clear existing state
            var updateResult = _referenceService.UpdateExternalReferences(
                ReferenceRootType.SystemUsage,
                systemUsage.Id,
                externalReferences);

            if (updateResult.HasValue)
                return new OperationError($"Failed to update references with error message: {updateResult.Value.Message.GetValueOrEmptyString()}", updateResult.Value.FailureType);

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

        private Maybe<OperationError> UpdateArchiveJournalPeriods(ItSystemUsage systemUsage, Maybe<IEnumerable<SystemUsageJournalPeriodUpdate>> journalPeriods)
        {
            var allJournalPeriods = journalPeriods
                .Select(x => x.ToList())
                .GetValueOrFallback(new List<SystemUsageJournalPeriodUpdate>());


            var systemUsageJournalPeriodsWithUuid = allJournalPeriods.Where(x => x.Uuid.HasValue).ToList();
            if (systemUsageJournalPeriodsWithUuid.Count != systemUsageJournalPeriodsWithUuid.Select(x => x.Uuid.GetValueOrDefault()).Distinct().Count())
            {
                return new OperationError("It's not allowed to have duplicate uuids in the journal periods provided in the update", OperationFailure.BadInput);
            }
            var specificJournalPeriodUpdates = systemUsageJournalPeriodsWithUuid
                .ToDictionary(x => x.Uuid.GetValueOrDefault());

            var journalPeriodsToAddAfterReset = allJournalPeriods
                .Except(specificJournalPeriodUpdates.Values)
                .ToList();

            if (specificJournalPeriodUpdates.Any())
            {
                //Update the journal periods specifically identified in the update
                foreach (var specificJournalPeriodUpdate in specificJournalPeriodUpdates)
                {
                    var period = specificJournalPeriodUpdate.Value;
                    var updateResult = _systemUsageService.UpdateArchivePeriod(systemUsage.Id, specificJournalPeriodUpdate.Key, period.StartDate, period.EndDate, period.ArchiveId, period.Approved);
                    if (updateResult.Failed)
                        return new OperationError($"Failed to update ArchiveJournalPeriod {period.Uuid} as part of the update. Update error: {updateResult.Error.Message.GetValueOrEmptyString()}", updateResult.Error.FailureType);
                }

                //Remove journal periods which were not identified as specific updates
                var uuidsOfJournalPeriodsToRemove = systemUsage
                    .ArchivePeriods
                    .Where(period => !specificJournalPeriodUpdates.ContainsKey(period.Uuid))
                    .Select(period => period.Uuid)
                    .ToList();
                foreach (var uuid in uuidsOfJournalPeriodsToRemove)
                {
                    var removeResult = _systemUsageService.RemoveArchivePeriod(systemUsage.Id, uuid);
                    if (removeResult.Failed)
                        return new OperationError($"Failed to remove ArchiveJournalPeriod {uuid} as part of the update. Remove error: {removeResult.Error.Message.GetValueOrEmptyString()}", removeResult.Error.FailureType);
                }
            }
            else
            {
                //If no specific updates, just remove all
                var removeResult = _systemUsageService.RemoveAllArchivePeriods(systemUsage.Id);
                if (removeResult.Failed)
                    return new OperationError($"Failed to remove all ArchiveJournalPeriods as part of the update. Remove error: {removeResult.Error.Message.GetValueOrEmptyString()}", removeResult.Error.FailureType);
            }

            //Add periods which were not part of the specific update set.
            foreach (var newPeriod in journalPeriodsToAddAfterReset)
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

            //Not a change from current state so do not apply availability constraint
            if (systemUsage.ArchiveSupplierId != null && systemUsage.ArchiveSupplierId == orgByUuid.Value.Id)
                return Maybe<OperationError>.None;

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
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.LifeCycleStatus, (systemUsage, lifeCycleStatus) => systemUsage.LifeCycleStatus = lifeCycleStatus))
                .Bind(usage => UpdateValidityPeriod(usage, generalProperties))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.MainContractUuid, UpdateMainContract))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.ContainsAITechnology, (systemUsage, containsAITechnology) => systemUsage.UpdateContainsAITechnology(containsAITechnology)))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.WebAccessibilityCompliance, (systemUsage, webAccessibilityCompliance) => systemUsage.UpdateWebAccessibilityCompliance(webAccessibilityCompliance)))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.LastWebAccessibilityCheck, (systemUsage, lastWebAccessibilityCheck) => systemUsage.UpdateLastWebAccessibilityCheck(lastWebAccessibilityCheck)))
                .Bind(usage => usage.WithOptionalUpdate(generalProperties.WebAccessibilityNotes, (systemUsage, webAccessibilityNotes) => systemUsage.UpdateWebAccessibilityNotes(webAccessibilityNotes)));
        }

        private static Result<ItSystemUsage, OperationError> UpdateValidityPeriod(ItSystemUsage usage, UpdatedSystemUsageGeneralProperties generalProperties)
        {
            if (generalProperties.ValidFrom.IsUnchanged && generalProperties.ValidTo.IsUnchanged)
                return usage; //Not changes provided

            var newValidFrom = generalProperties.ValidFrom.MapDateTimeOptionalChangeWithFallback(usage.Concluded);
            var newValidTo = generalProperties.ValidTo.MapDateTimeOptionalChangeWithFallback(usage.ExpirationDate);

            return usage.UpdateSystemValidityPeriod(newValidFrom, newValidTo).Match<Result<ItSystemUsage, OperationError>>(error => error, () => usage);
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
            var roleAssignments = userRolePairs
                .Select(x => x.Select(pair => (pair.RoleUuid, pair.UserUuid)))
                .GetValueOrFallback(new List<(Guid RoleUuid, Guid UserUuid)>());

            return _roleAssignmentService
                .BatchUpdateRoles(systemUsage, roleAssignments)
                .Match<Result<ItSystemUsage, OperationError>>(error => error, () => systemUsage);
        }

        private Result<ItSystemUsage, OperationError> GetUsageAndAuthorizeWriteAccess(Guid uuid)
        {
            return _systemUsageService.GetReadableItSystemUsageByUuid(uuid)
                .Bind(WithWriteAccess);
        }

        private Result<ItSystemUsage, OperationError> WithWriteAccess(ItSystemUsage systemUsage)
        {
            return _authorizationContext.AllowModify(systemUsage) ? systemUsage : new OperationError(OperationFailure.Forbidden);
        }

        public Maybe<OperationError> Delete(Guid itSystemUsageUuid)
        {
            return _systemUsageService.GetReadableItSystemUsageByUuid(itSystemUsageUuid)
                .Match(DeleteUsage, error => error);
        }

        public Maybe<OperationError> DeleteByItSystemAndOrganizationUuids(Guid itSystemUuid, Guid organizationUuid)
        {
            return _systemService.GetSystem(itSystemUuid)
                .Bind(system => _organizationService.GetOrganization(organizationUuid).Select(organization => (system, organization)))
                .Match(result =>
                {
                    var usage = _systemUsageService.GetByOrganizationAndSystemId(result.organization.Id, result.system.Id);
                    return DeleteUsage(usage);
                }, error => error);
        }

        public Result<SystemRelation, OperationError> CreateSystemRelation(Guid fromSystemUsageUuid, SystemRelationParameters parameters)
        {
            return _systemUsageService.GetReadableItSystemUsageByUuid(fromSystemUsageUuid)
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
                .Bind(ids => ResolveRequiredId<ItSystemUsage>(parameters.ToSystemUsageUuid).Select(id => (ids.contractId, ids.interfaceId, ids.frequencyId, systemUsageId: id)));
        }

        public Result<SystemRelation, OperationError> UpdateSystemRelation(Guid fromSystemUsageUuid, Guid relationUuid, SystemRelationParameters parameters)
        {
            return _systemUsageService.GetReadableItSystemUsageByUuid(fromSystemUsageUuid)
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
                .GetReadableItSystemUsageByUuid(itSystemUsageUuid)
                .Bind<(int usageId, int relationId)>(usage =>
                {
                    var usageRelation = _identityResolver.ResolveDbId<SystemRelation>(itSystemUsageRelationUuid);
                    if (usageRelation.IsNone)
                        return new OperationError(
                            $"Relation with id:{itSystemUsageRelationUuid} does not exist", OperationFailure.BadInput);
                    return (usage.Id, usageRelation.Value);
                })
                .Bind(usageAndRelation => _systemUsageRelationsService.RemoveRelation(usageAndRelation.usageId, usageAndRelation.relationId))
                .MatchFailure();
        }

        public Result<ArchivePeriod, OperationError> CreateJournalPeriod(Guid systemUsageUuid, SystemUsageJournalPeriodProperties parameters)
        {
            return _systemUsageService
                .GetReadableItSystemUsageByUuid(systemUsageUuid)
                .Bind(WithWriteAccess)
                .Bind(usage => _systemUsageService.AddArchivePeriod(usage.Id, parameters.StartDate, parameters.EndDate, parameters.ArchiveId, parameters.Approved));
        }

        public Result<ArchivePeriod, OperationError> UpdateJournalPeriod(Guid systemUsageUuid, Guid periodUuid, SystemUsageJournalPeriodProperties parameters)
        {
            return _systemUsageService
                .GetReadableItSystemUsageByUuid(systemUsageUuid)
                .Bind(WithWriteAccess)
                .Bind(usage => _systemUsageService.UpdateArchivePeriod(usage.Id, periodUuid, parameters.StartDate, parameters.EndDate, parameters.ArchiveId, parameters.Approved));
        }

        public Result<ArchivePeriod, OperationError> DeleteJournalPeriod(Guid systemUsageUuid, Guid periodUuid)
        {
            return _systemUsageService
                .GetReadableItSystemUsageByUuid(systemUsageUuid)
                .Bind(WithWriteAccess)
                .Bind(usage => _systemUsageService.RemoveArchivePeriod(usage.Id, periodUuid));
        }

        private Maybe<OperationError> DeleteUsage(ItSystemUsage usage)
        {
            return _systemUsageService.Delete(usage.Id)
                .Match(_ => Maybe<OperationError>.None, error => new OperationError($"Failed to delete it system usage with Uuid: {usage.Uuid}, Error message: {error.Message.GetValueOrEmptyString()}", error.FailureType));
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

        private static IReadOnlyList<UserRolePair> ExtractAssignedRoles(ItSystemUsage systemUsage)
        {
            return systemUsage.Rights.Select(right => new UserRolePair(right.User.Uuid, right.Role.Uuid)).ToList();
        }

        private static SystemUsageUpdateParameters CreateRoleAssignmentUpdate(IEnumerable<UserRolePair> existingRoles)
        {
            return new SystemUsageUpdateParameters
            {
                Roles = new UpdatedSystemUsageRoles
                {
                    UserRolePairs = existingRoles.FromNullable().AsChangedValue()
                }
            };
        }
    }
}
