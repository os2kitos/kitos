using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Generic.Write;
using Core.ApplicationServices.Helpers;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.GDPR.Write.SubDataProcessor;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Shared;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;

using Serilog;

namespace Core.ApplicationServices.GDPR.Write
{
    public class DataProcessingRegistrationWriteService : IDataProcessingRegistrationWriteService
    {
        private readonly IDataProcessingRegistrationApplicationService _applicationService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IReferenceService _referenceService;
        private readonly ILogger _logger;
        private readonly IDomainEvents _domainEvents;
        private readonly ITransactionManager _transactionManager;
        private readonly IDatabaseControl _databaseControl;
        private readonly IAssignmentUpdateService _assignmentUpdateService;

        public DataProcessingRegistrationWriteService(
            IDataProcessingRegistrationApplicationService applicationService,
            IEntityIdentityResolver entityIdentityResolver,
            IReferenceService referenceService,
            ILogger logger,
            IDomainEvents domainEvents,
            ITransactionManager transactionManager,
            IDatabaseControl databaseControl,
            IAssignmentUpdateService assignmentUpdateService)
        {
            _applicationService = applicationService;
            _entityIdentityResolver = entityIdentityResolver;
            _referenceService = referenceService;
            _logger = logger;
            _domainEvents = domainEvents;
            _transactionManager = transactionManager;
            _databaseControl = databaseControl;
            _assignmentUpdateService = assignmentUpdateService;
        }

        public Result<DataProcessingRegistration, OperationError> Create(Guid organizationUuid, DataProcessingRegistrationModificationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            using var transaction = _transactionManager.Begin();

            var orgId = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);

            if (orgId.IsNone)
            {
                _logger.Error("Failed to retrieve organization with id {uuid}");
                return new OperationError($"Unable to resolve Organization with uuid{organizationUuid}", OperationFailure.BadInput);
            }
            if (parameters.Name.IsUnchanged)
                return new OperationError("Name must be provided", OperationFailure.BadInput);

            var name = parameters.Name.NewValue;

            parameters.Name = OptionalValueChange<string>.None; //Remove from change set. It is set during creation

            var creationResult = _applicationService
                .Create(orgId.Value, name)
                .Bind(createdDpr => Update(() => createdDpr, parameters));

            if (creationResult.Ok)
            {
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return creationResult;
        }

        public Result<DataProcessingRegistration, OperationError> Update(Guid dataProcessingRegistrationUuid, DataProcessingRegistrationModificationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return Update(() => _applicationService.GetByUuid(dataProcessingRegistrationUuid), parameters);
        }

        public Result<DataProcessingRegistration, OperationError> AddRole(Guid dprUuid, UserRolePair assignment)
        {
            return AddRoles(dprUuid, assignment.WrapAsEnumerable());
        }

        public Result<DataProcessingRegistration, OperationError> AddRoleRange(Guid dprUuid,
            IEnumerable<UserRolePair> assignments)
        {
            return AddRoles(dprUuid, assignments);
        }

        public Result<DataProcessingRegistration, OperationError> RemoveRole(Guid dprUuid, UserRolePair assignment)
        {
            return _applicationService
                .GetByUuid(dprUuid)
                .Select(RoleMappingHelper.ExtractAssignedRoles)
                .Bind<DataProcessingRegistrationModificationParameters>(existingRoles =>
                {
                    if (!existingRoles.Contains(assignment))
                    {
                        return new OperationError("Assignment does not exist", OperationFailure.BadInput);
                    }
                    return CreateRoleAssignmentUpdate(existingRoles.Except(assignment.WrapAsEnumerable()));
                })
                .Bind(update => Update(dprUuid, update));
        }

        private Result<DataProcessingRegistration, OperationError> AddRoles(Guid dprUuid,
            IEnumerable<UserRolePair> assignments)
        {
            return _applicationService
                .GetByUuid(dprUuid)
                .Bind(dpr => GetRoleAssignmentUpdates(dpr, assignments))
                .Bind(update => Update(dprUuid, update));
        }

        private Result<DataProcessingRegistration, OperationError> Update(Func<Result<DataProcessingRegistration, OperationError>> getDpr, DataProcessingRegistrationModificationParameters parameters)
        {
            using var transaction = _transactionManager.Begin();

            var dprResult = getDpr();

            if (dprResult.Failed)
            {
                return dprResult.Error;
            }

            var dpr = dprResult.Value;
            var snapshot = dpr.Snapshot();

            var result = PerformUpdates(dpr, parameters);

            if (result.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEventWithSnapshot<DataProcessingRegistration, DprSnapshot>(result.Value, snapshot.FromNullable()));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return result;
        }

        private Result<DataProcessingRegistration, OperationError> PerformUpdates(DataProcessingRegistration dpr, DataProcessingRegistrationModificationParameters parameters)
        {
            //Optionally apply changes across the entire update specification
            return dpr
                .WithOptionalUpdate(parameters.Name, (registration, changedName) => _applicationService.UpdateName(registration.Id, changedName))
                .Bind(registration => registration.WithOptionalUpdate(parameters.General, UpdateGeneralData))
                .Bind(registration => registration.WithOptionalUpdate(parameters.SystemUsageUuids, UpdateSystemAssignments))
                .Bind(registration => registration.WithOptionalUpdate(parameters.Oversight, UpdateOversightData))
                .Bind(registration => registration.WithOptionalUpdate(parameters.Roles, UpdateRolesData))
                .Bind(registration => registration.WithOptionalUpdate(parameters.ExternalReferences, PerformReferencesUpdate));
        }

        private Result<DataProcessingRegistration, OperationError> PerformReferencesUpdate(DataProcessingRegistration dpr, IEnumerable<UpdatedExternalReferenceProperties> externalReferences)
        {
            var updateResult = _referenceService.UpdateExternalReferences(
                ReferenceRootType.DataProcessingRegistration,
                dpr.Id,
                externalReferences.ToList());

            if (updateResult.HasValue)
                return new OperationError($"Failed to update references with error message: {updateResult.Value.Message.GetValueOrEmptyString()}", updateResult.Value.FailureType);

            return dpr;
        }

        private Result<DataProcessingRegistration, OperationError> UpdateRolesData(DataProcessingRegistration dpr, UpdatedDataProcessingRegistrationRoles dprRoles)
        {
            return dpr.WithOptionalUpdate(dprRoles.UserRolePairs, UpdateRoles);
        }

        private Result<DataProcessingRegistration, OperationError> UpdateRoles(DataProcessingRegistration dpr, Maybe<IEnumerable<UserRolePair>> userRolePairs)
        {
            var newRightsList = userRolePairs.GetValueOrFallback(new List<UserRolePair>()).ToList();
            if (newRightsList.Distinct().Count() != newRightsList.Count)
            {
                return new OperationError($"Duplicates of 'User Role Pairs' are not allowed", OperationFailure.BadInput);
            }

            var existingRightsList = dpr.Rights.Select(x => new UserRolePair(x.User.Uuid, x.Role.Uuid)).ToList();

            foreach (var (delta, item) in existingRightsList.ComputeDelta(newRightsList, x => x))
            {
                var userId = _entityIdentityResolver.ResolveDbId<User>(item.UserUuid);
                if (userId.IsNone)
                    return new OperationError($"Could not find user with Uuid: {item.UserUuid}", OperationFailure.BadInput);

                var roleId = _entityIdentityResolver.ResolveDbId<DataProcessingRegistrationRole>(item.RoleUuid);
                if (roleId.IsNone)
                    return new OperationError($"Could not find role with Uuid: {item.RoleUuid}", OperationFailure.BadInput);

                switch (delta)
                {
                    case EnumerableExtensions.EnumerableDelta.Added:
                        var assignmentResult = _applicationService.AssignRole(dpr.Id, roleId.Value, userId.Value);
                        if (assignmentResult.Failed)
                            return new OperationError($"Failed to assign role with Uuid: {item.RoleUuid} from user with Uuid: {item.UserUuid}, with following error message: {assignmentResult.Error.Message.GetValueOrEmptyString()}", assignmentResult.Error.FailureType);
                        break;

                    case EnumerableExtensions.EnumerableDelta.Removed:
                        var removeResult = _applicationService.RemoveRole(dpr.Id, roleId.Value, userId.Value);
                        if (removeResult.Failed)
                            return new OperationError($"Failed to remove role with Uuid: {item.RoleUuid} from user with Uuid: {item.UserUuid}, with following error message: {removeResult.Error.Message.GetValueOrEmptyString()}", removeResult.Error.FailureType);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return dpr;
        }

        private Result<DataProcessingRegistration, OperationError> UpdateOversightData(DataProcessingRegistration dpr, UpdatedDataProcessingRegistrationOversightDataParameters parameters)
        {
            return dpr
                .WithOptionalUpdate(parameters.OversightOptionUuids, UpdateOversightOptions)
                .Bind(r => r.WithOptionalUpdate(parameters.OversightOptionsRemark, (registration, remark) => _applicationService.UpdateOversightOptionRemark(registration.Id, remark)))
                .Bind(r => r.WithOptionalUpdate(parameters.OversightInterval, (registration, interval) => _applicationService.UpdateOversightInterval(registration.Id, interval ?? YearMonthIntervalOption.Undecided)))
                .Bind(r => r.WithOptionalUpdate(parameters.OversightIntervalRemark, (registration, remark) => _applicationService.UpdateOversightIntervalRemark(registration.Id, remark)))
                .Bind(r => r.WithOptionalUpdate(parameters.IsOversightCompleted, (registration, completed) => _applicationService.UpdateIsOversightCompleted(registration.Id, completed ?? YesNoUndecidedOption.Undecided)))
                .Bind(r => r.WithOptionalUpdate(parameters.OversightCompletedRemark, (registration, remark) => _applicationService.UpdateOversightCompletedRemark(registration.Id, remark)))
                .Bind(r => r.WithOptionalUpdate(parameters.OversightScheduledInspectionDate, (registration, date) => _applicationService.UpdateOversightScheduledInspectionDate(registration.Id, date)))
                .Bind(r => r.WithOptionalUpdate(parameters.OversightDates, UpdateOversightDates));
        }

        private Maybe<OperationError> UpdateOversightDates(DataProcessingRegistration dpr, Maybe<IEnumerable<UpdatedDataProcessingRegistrationOversightDate>> oversightDates)
        {
            // As this is "dumb" data (i.e. we don't know if two with equal data is supposed to be the same or two different registrations) we first remove all before assigning new values.
            var oldDates = dpr.OversightDates.ToList();
            foreach (var oldDate in oldDates)
            {
                var removeResult = _applicationService.RemoveOversightDate(dpr.Id, oldDate.Id);

                if (removeResult.Failed)
                    return new OperationError($"Failed to remove old oversight date with Id: {oldDate.Id}. Error message: {removeResult.Error.Message.GetValueOrEmptyString()}", removeResult.Error.FailureType);
            }

            if (oversightDates.IsNone)
                return Maybe<OperationError>.None;

            foreach (var newDate in oversightDates.Value)
            {
                var assignResult = _applicationService.AssignOversightDate(dpr.Id, newDate.CompletedAt, newDate.Remark);

                if (assignResult.Failed)
                    return new OperationError($"Failed to assign new oversight date with Date: {newDate.CompletedAt} and Remark: {newDate.Remark}. Error message: {assignResult.Error.Message.GetValueOrEmptyString()}", assignResult.Error.FailureType);
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> UpdateOversightOptions(DataProcessingRegistration dpr, Maybe<IEnumerable<Guid>> oversightOptionUuids)
        {
            return _assignmentUpdateService.UpdateUniqueMultiAssignment
            (
                "oversight options",
                dpr,
                oversightOptionUuids,
                oversightUuid => _entityIdentityResolver.ResolveDbId<DataProcessingOversightOption>(oversightUuid).Match<Result<int, OperationError>>(optionId => optionId, () => new OperationError($"Failed to resolve Id for Uuid {oversightUuid}", OperationFailure.BadInput)),
                registration => registration.OversightOptions.ToList(),
                (registration, oversightOptionId) => _applicationService.AssignOversightOption(registration.Id, oversightOptionId).MatchFailure(),
                (registration, oversightOption) => _applicationService.RemoveOversightOption(registration.Id, oversightOption.Id).MatchFailure()
            );
        }

        private Result<DataProcessingRegistration, OperationError> UpdateGeneralData(DataProcessingRegistration dpr, UpdatedDataProcessingRegistrationGeneralDataParameters parameters)
        {
            return dpr
                .WithOptionalUpdate(parameters.DataResponsibleUuid, UpdateDataResponsible)
                .Bind(r => r.WithOptionalUpdate(parameters.DataResponsibleRemark, (registration, remark) => _applicationService.UpdateDataResponsibleRemark(registration.Id, remark)))
                .Bind(r => r.WithOptionalUpdate(parameters.IsAgreementConcluded, (registration, concluded) => _applicationService.UpdateIsAgreementConcluded(registration.Id, concluded ?? YesNoIrrelevantOption.UNDECIDED)))
                .Bind(r => r.WithOptionalUpdate(parameters.IsAgreementConcludedRemark, (registration, concludedRemark) => _applicationService.UpdateAgreementConcludedRemark(registration.Id, concludedRemark)))
                .Bind(r => r.WithOptionalUpdate(parameters.AgreementConcludedAt, (registration, concludedAt) => _applicationService.UpdateAgreementConcludedAt(registration.Id, concludedAt)))
                .Bind(r => r.WithOptionalUpdate(parameters.BasisForTransferUuid, UpdateBasisForTransfer))
                .Bind(r => r.WithOptionalUpdate(parameters.TransferToInsecureThirdCountries, (registration, newValue) => _applicationService.UpdateTransferToInsecureThirdCountries(registration.Id, newValue ?? YesNoUndecidedOption.Undecided)))
                .Bind(r => r.WithOptionalUpdate(parameters.InsecureCountriesSubjectToDataTransferUuids, UpdateInsecureCountriesSubjectToDataTransfer))
                .Bind(r => r.WithOptionalUpdate(parameters.DataProcessorUuids, UpdateDataProcessors))
                .Bind(r => r.WithOptionalUpdate(parameters.HasSubDataProcessors, (registration, newValue) => _applicationService.SetSubDataProcessorsState(registration.Id, newValue ?? YesNoUndecidedOption.Undecided)))
                .Bind(r => r.WithOptionalUpdate(parameters.SubDataProcessors, UpdateSubDataProcessors))
                .Bind(r => r.WithOptionalUpdate(parameters.MainContractUuid, UpdateMainContract))
                .Bind(r => r.WithOptionalUpdate(parameters.ResponsibleUnitUuid, UpdateResponsibleUnit));
        }

        private Result<DataProcessingRegistration, OperationError> UpdateResponsibleUnit(DataProcessingRegistration dpr, Guid? orgUnitUuid)
        {
            if (orgUnitUuid == null)
            {
                dpr.ResetResponsibleOrganizationUnit();
                return dpr;
            }
            var updateResult = dpr.SetResponsibleOrganizationUnit(orgUnitUuid.Value);
            return updateResult.Match<Result<DataProcessingRegistration, OperationError>>(err => err, () => dpr);

        }

        private Result<DataProcessingRegistration, OperationError> UpdateSystemAssignments(DataProcessingRegistration dpr, IEnumerable<Guid> systemUsageUuids)
        {
            return _assignmentUpdateService.UpdateUniqueMultiAssignment
            (
                "system usage",
                dpr,
                systemUsageUuids.FromNullable(),
                usageUuid => _entityIdentityResolver.ResolveDbId<ItSystemUsage>(usageUuid).Match<Result<int, OperationError>>(optionId => optionId, () => new OperationError($"Failed to resolve Id for Uuid {usageUuid}", OperationFailure.BadInput)),
                registration => registration.SystemUsages.ToList(),
                (registration, usageId) => _applicationService.AssignSystem(registration.Id, usageId).MatchFailure(),
                (registration, usage) => _applicationService.RemoveSystem(registration.Id, usage.Id).MatchFailure()
            ).Match<Result<DataProcessingRegistration, OperationError>>(error => error, () => dpr);
        }

        private Maybe<OperationError> UpdateSubDataProcessors(DataProcessingRegistration dpr, Maybe<IEnumerable<SubDataProcessorParameter>> subDataProcessors)
        {
            var basisForTransferLookups = new Dictionary<Guid, int>();
            var countryIdLookups = new Dictionary<Guid, int>();
            var orgIdLookup = new Dictionary<int, Guid>();

            foreach (var subDataProcessorParameter in subDataProcessors.GetValueOrFallback(new List<SubDataProcessorParameter>()))
            {
                var basisForTransferOptionUuid = subDataProcessorParameter.BasisForTransferOptionUuid;
                if (basisForTransferOptionUuid.HasValue)
                {
                    var optionUuid = basisForTransferOptionUuid.Value;
                    if (!basisForTransferLookups.ContainsKey(optionUuid))
                    {
                        var dbId = _entityIdentityResolver.ResolveDbId<DataProcessingBasisForTransferOption>(optionUuid);
                        if (dbId.IsNone)
                            return new OperationError($"Provided id for basis for transfer {optionUuid} does not point to a valid entity", OperationFailure.BadInput);
                        basisForTransferLookups.Add(optionUuid, dbId.Value);
                    }
                }

                var insecureCountryParam = subDataProcessorParameter.InsecureCountrySubjectToDataTransferUuid;
                if (insecureCountryParam.HasValue)
                {
                    var optionUuid = insecureCountryParam.Value;
                    if (!countryIdLookups.ContainsKey(optionUuid))
                    {
                        var dbId = _entityIdentityResolver.ResolveDbId<DataProcessingCountryOption>(optionUuid);
                        if (dbId.IsNone)
                            return new OperationError($"Provided id for country {optionUuid} does not point to a valid entity", OperationFailure.BadInput);
                        countryIdLookups.Add(optionUuid, dbId.Value);
                    }
                }

                var organizationUuid = subDataProcessorParameter.OrganizationUuid;
                if (!orgIdLookup.ContainsValue(organizationUuid))
                {
                    var orgId = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
                    if (orgId.IsNone)
                        return new OperationError($"Provided org id {organizationUuid} does not point to a valid entity", OperationFailure.BadInput);
                    orgIdLookup.Add(orgId.Value, organizationUuid);
                }
            }

            var detailsLookup = subDataProcessors
                .Select(x => x.ToDictionary(sdp => sdp.OrganizationUuid, sdp => ToSubDataProcessorDetailsParameters(sdp, basisForTransferLookups, countryIdLookups)))
                .GetValueOrFallback(new Dictionary<Guid, SubDataProcessorDetailsParameters>());

            return _assignmentUpdateService.UpdateUniqueMultiAssignment
            (
                "sub data processor",
                dpr,
                subDataProcessors.Select<IEnumerable<Guid>>(x => x.Select(sdp => sdp.OrganizationUuid).ToList()),
                subDataProcessorUuid => _entityIdentityResolver.ResolveDbId<Organization>(subDataProcessorUuid).Match<Result<int, OperationError>>(optionId => optionId, () => new OperationError($"Failed to resolve Id for Uuid {subDataProcessorUuid}", OperationFailure.BadInput)),
                registration => registration.AssignedSubDataProcessors.Select(x => x.Organization).ToList(),
                (registration, subDataProcessorId) => _applicationService.AssignSubDataProcessor(registration.Id, subDataProcessorId, detailsLookup[orgIdLookup[subDataProcessorId]]).MatchFailure(),
                (registration, subDataProcessor) => _applicationService.RemoveSubDataProcessor(registration.Id, subDataProcessor.Id).MatchFailure(),
                update: (registration, subDataProcessor) => _applicationService.UpdateSubDataProcessor(registration.Id, subDataProcessor.Id, detailsLookup[subDataProcessor.Uuid]).MatchFailure()
                );
        }

        private static SubDataProcessorDetailsParameters ToSubDataProcessorDetailsParameters(SubDataProcessorParameter sdp, IReadOnlyDictionary<Guid, int> basisForTransferLookups, Dictionary<Guid, int> countryIdLookups)
        {
            return new SubDataProcessorDetailsParameters(sdp.BasisForTransferOptionUuid?.Transform(id => basisForTransferLookups[id]), ToInsecureCountryParameters(sdp, countryIdLookups));
        }

        private static TransferToInsecureCountryParameters ToInsecureCountryParameters(SubDataProcessorParameter sdp, IReadOnlyDictionary<Guid, int> countryIdLookups)
        {
            return new TransferToInsecureCountryParameters(sdp.TransferToInsecureThirdCountry, sdp.InsecureCountrySubjectToDataTransferUuid?.Transform(id => countryIdLookups[id]));
        }

        private Maybe<OperationError> UpdateDataProcessors(DataProcessingRegistration dpr, Maybe<IEnumerable<Guid>> organizationUuids)
        {
            return _assignmentUpdateService.UpdateUniqueMultiAssignment
            (
                "data processor",
                dpr,
                organizationUuids,
                dataProcessorUuid => _entityIdentityResolver.ResolveDbId<Organization>(dataProcessorUuid).Match<Result<int, OperationError>>(optionId => optionId, () => new OperationError($"Failed to resolve Id for Uuid {dataProcessorUuid}", OperationFailure.BadInput)),
                registration => registration.DataProcessors.ToList(),
                (registration, dataProcessorId) => _applicationService.AssignDataProcessor(registration.Id, dataProcessorId).MatchFailure(),
                (registration, dataProcessor) => _applicationService.RemoveDataProcessor(registration.Id, dataProcessor.Id).MatchFailure()
            );
        }

        private Maybe<OperationError> UpdateInsecureCountriesSubjectToDataTransfer(DataProcessingRegistration dpr, Maybe<IEnumerable<Guid>> countryOptionUuids)
        {
            return _assignmentUpdateService.UpdateUniqueMultiAssignment
            (
                "insecure third country",
                dpr,
                countryOptionUuids,
                optionUuid => _entityIdentityResolver.ResolveDbId<DataProcessingCountryOption>(optionUuid).Match<Result<int, OperationError>>(optionId => optionId, () => new OperationError($"Failed to resolve Id for Uuid {optionUuid}", OperationFailure.BadInput)),
                registration => registration.InsecureCountriesSubjectToDataTransfer.ToList(),
                (registration, countryOptionId) => _applicationService.AssignInsecureThirdCountry(registration.Id, countryOptionId).MatchFailure(),
                (registration, countryOption) => _applicationService.RemoveInsecureThirdCountry(registration.Id, countryOption.Id).MatchFailure()
            );
        }

        private Maybe<OperationError> UpdateBasisForTransfer(DataProcessingRegistration dpr, Guid? basisForTransferUuid)
        {
            if (!basisForTransferUuid.HasValue)
                return _applicationService
                    .ClearBasisForTransfer(dpr.Id)
                    .Match(
                        _ => Maybe<OperationError>.None,
                        error => error.FailureType == OperationFailure.BadState ? Maybe<OperationError>.None : error
                    );

            var dbId = _entityIdentityResolver.ResolveDbId<DataProcessingBasisForTransferOption>(basisForTransferUuid.Value);

            if (dbId.IsNone)
                return new OperationError($"Basis for transfer option with uuid {basisForTransferUuid.Value} could not be found", OperationFailure.BadInput);

            return _applicationService
                .AssignBasisForTransfer(dpr.Id, dbId.Value)
                .MatchFailure();
        }

        private Result<DataProcessingRegistration, OperationError> UpdateMainContract(DataProcessingRegistration dpr, Guid? contractUuid)
        {
            if (contractUuid.HasValue)
            {
                return _entityIdentityResolver.ResolveDbId<ItContract>(contractUuid.Value)
                    .Match
                    (
                        contractId => _applicationService.UpdateMainContract(dpr.Id, contractId),
                        () => new OperationError($"It contract with uuid {contractUuid.Value} could not be found", OperationFailure.BadInput)
                    );
            }
            return _applicationService.RemoveMainContract(dpr.Id);

        }

        private Maybe<OperationError> UpdateDataResponsible(DataProcessingRegistration dpr, Guid? dataResponsibleUuid)
        {
            if (!dataResponsibleUuid.HasValue)
                return _applicationService
                    .ClearDataResponsible(dpr.Id)
                    .Match(
                        _ => Maybe<OperationError>.None,
                        error => error.FailureType == OperationFailure.BadState ? Maybe<OperationError>.None : error
                    );

            var dbId = _entityIdentityResolver.ResolveDbId<DataProcessingDataResponsibleOption>(dataResponsibleUuid.Value);

            if (dbId.IsNone)
                return new OperationError($"Data responsible option with uuid {dataResponsibleUuid.Value} could not be found", OperationFailure.BadInput);

            return _applicationService
                .AssignDataResponsible(dpr.Id, dbId.Value)
                .MatchFailure();
        }

        public Maybe<OperationError> Delete(Guid dataProcessingRegistrationUuid)
        {
            var dbId = _entityIdentityResolver.ResolveDbId<DataProcessingRegistration>(dataProcessingRegistrationUuid);

            if (dbId.IsNone)
                return new OperationError(OperationFailure.NotFound);

            return _applicationService
                .Delete(dbId.Value)
                .Match(_ => Maybe<OperationError>.None, error => error);
        }
        
        private static Result<DataProcessingRegistrationModificationParameters, OperationError> GetRoleAssignmentUpdates(DataProcessingRegistration dpr, IEnumerable<UserRolePair> assignments)
        {
            var existingRoles = RoleMappingHelper.ExtractAssignedRoles(dpr);
            var newRoles = assignments.ToList();

            if (existingRoles.Any(newRoles.Contains))
            {
                return new OperationError("Role assignment exists", OperationFailure.Conflict);
            }
            
            return CreateRoleAssignmentUpdate(existingRoles.Concat(newRoles));
        }


        private static DataProcessingRegistrationModificationParameters CreateRoleAssignmentUpdate(IEnumerable<UserRolePair> existingRoles)
        {
            return new DataProcessingRegistrationModificationParameters
            {
                Roles = new UpdatedDataProcessingRegistrationRoles
                {
                    UserRolePairs = existingRoles.FromNullable().AsChangedValue()
                }
            };
        }
    }
}
