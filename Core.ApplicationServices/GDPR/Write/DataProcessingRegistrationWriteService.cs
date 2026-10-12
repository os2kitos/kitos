using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Generic.Write;
using Core.ApplicationServices.Helpers;
using Core.ApplicationServices.Model;
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
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.GDPR;
using Core.DomainServices.Generic;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Role;
using Infrastructure.Services.DataAccess;

using Serilog;

namespace Core.ApplicationServices.GDPR.Write
{
    public class DataProcessingRegistrationWriteService : IDataProcessingRegistrationWriteService
    {
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IReferenceService _referenceService;
        private readonly ILogger _logger;
        private readonly IDomainEvents _domainEvents;
        private readonly ITransactionManager _transactionManager;
        private readonly IDatabaseControl _databaseControl;
        private readonly IAssignmentUpdateService _assignmentUpdateService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IDataProcessingRegistrationNamingService _namingService;
        private readonly IDataProcessingRegistrationRepository _repository;
        private readonly IGenericRepository<DataProcessingRegistrationOversightDate> _oversightDateRepository;
        private readonly IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration> _roleAssignmentsService;
        private readonly IGenericRepository<DataProcessingRegistrationOversightDate> _dataProcessingRegistrationOversightDateRepository;
        private readonly IDataProcessingRegistrationOversightOptionsAssignmentService _oversightOptionAssignmentService;
        private readonly IGenericRepository<SubDataProcessor> _sdpRepository;
        private readonly IDataProcessingRegistrationSystemAssignmentService _systemAssignmentService;
        private readonly IDataProcessingRegistrationDataProcessorAssignmentService _dataProcessingRegistrationDataProcessorAssignmentService;
        private readonly IDataProcessingRegistrationInsecureCountriesAssignmentService _countryAssignmentService;
        private readonly IDataProcessingRegistrationBasisForTransferAssignmentService _basisForTransferAssignmentService;
        private readonly IDataProcessingRegistrationDataResponsibleAssignmentService _dataResponsibleAssignmentService;



        public DataProcessingRegistrationWriteService(
            IEntityIdentityResolver entityIdentityResolver,
            IReferenceService referenceService,
            ILogger logger,
            IDomainEvents domainEvents,
            ITransactionManager transactionManager,
            IDatabaseControl databaseControl,
            IAssignmentUpdateService assignmentUpdateService, 
            IAuthorizationContext authorizationContext, IDataProcessingRegistrationNamingService namingService, IGenericRepository<DataProcessingRegistrationOversightDate> oversightDateRepository, IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration> roleAssignmentsService, IGenericRepository<DataProcessingRegistrationOversightDate> dataProcessingRegistrationOversightDateRepository, IDataProcessingRegistrationOversightOptionsAssignmentService oversightOptionAssignmentService, IGenericRepository<SubDataProcessor> sdpRepository, IDataProcessingRegistrationSystemAssignmentService systemAssignmentService, IDataProcessingRegistrationDataProcessorAssignmentService dataProcessingRegistrationDataProcessorAssignmentService, IDataProcessingRegistrationInsecureCountriesAssignmentService countryAssignmentService, IDataProcessingRegistrationBasisForTransferAssignmentService basisForTransferAssignmentService, IDataProcessingRegistrationDataResponsibleAssignmentService dataResponsibleAssignmentService, IDataProcessingRegistrationRepository repository)
        {
            _entityIdentityResolver = entityIdentityResolver;
            _referenceService = referenceService;
            _logger = logger;
            _domainEvents = domainEvents;
            _transactionManager = transactionManager;
            _databaseControl = databaseControl;
            _assignmentUpdateService = assignmentUpdateService;
            _authorizationContext = authorizationContext;
            _namingService = namingService;
            _oversightDateRepository = oversightDateRepository;
            _roleAssignmentsService = roleAssignmentsService;
            _dataProcessingRegistrationOversightDateRepository = dataProcessingRegistrationOversightDateRepository;
            _oversightOptionAssignmentService = oversightOptionAssignmentService;
            _sdpRepository = sdpRepository;
            _systemAssignmentService = systemAssignmentService;
            _dataProcessingRegistrationDataProcessorAssignmentService = dataProcessingRegistrationDataProcessorAssignmentService;
            _countryAssignmentService = countryAssignmentService;
            _basisForTransferAssignmentService = basisForTransferAssignmentService;
            _dataResponsibleAssignmentService = dataResponsibleAssignmentService;
            _repository = repository;
        }

        public Result<DataProcessingRegistration, OperationError> Create(Guid organizationUuid, DataProcessingRegistrationCreationParameters parameters)
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

            var creationResult = Create(orgId.Value, name)
                .Bind(createdDpr => PerformUpdates(createdDpr, parameters));

            if (creationResult.Ok)
            {
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return creationResult;
        }

        private Result<DataProcessingRegistration, OperationError> Create(int organizationId, string name)
        {
            if (!_authorizationContext.AllowCreate<DataProcessingRegistration>(organizationId))
                return new OperationError(OperationFailure.Forbidden);

            var error = _namingService.ValidateSuggestedNewRegistrationName(organizationId, name);

            if (error.HasValue)
                return error.Value;

            var registration = new DataProcessingRegistration
            {
                OrganizationId = organizationId,
                Name = name,
                Uuid = Guid.NewGuid()
            };

            var dataProcessingRegistration = _repository.Add(registration);
            return dataProcessingRegistration;
        }

        public Result<DataProcessingRegistration, OperationError> Update(Guid dataProcessingRegistrationUuid, DataProcessingRegistrationModificationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            return Update(() => GetByUuidAndAuthorizeRead(dataProcessingRegistrationUuid), PerformUpdates, parameters);
        }

        private Result<DataProcessingRegistration, OperationError> GetByUuidAndAuthorizeRead(Guid uuid)
        {
            return GetByUuid(uuid)
                .Bind
                (
                    dpr => _authorizationContext.AllowReads(dpr) ? Result<DataProcessingRegistration, OperationError>.Success(dpr) : new OperationError(OperationFailure.Forbidden)
                );
        }

        private Result<DataProcessingRegistration, OperationError> GetByUuid(Guid uuid)
        {
            return _repository
                .AsQueryable()
                .ByUuid(uuid)
                .FromNullable()
                .Match(Result<DataProcessingRegistration, OperationError>.Success,() => new OperationError($"Data Processing Registration with uuid: {uuid} was not found", OperationFailure.NotFound) );
        }

        public Result<DataProcessingRegistrationOversightDate, OperationError> AddOversightDate(Guid dataProcessingRegistrationUuid, UpdatedDataProcessingRegistrationOversightDateParameters parameters)
        {
            using var transaction = _transactionManager.Begin();

            var dprResult = GetByUuid(dataProcessingRegistrationUuid);

            if (dprResult.Failed)
            {
                return dprResult.Error;
            }

            var dpr = dprResult.Value;

            if (parameters is ISupplierAssociatedEntityUpdateParameters parametersAsSupplierAssociatedEntityUpdateParameters)
            {
                var authError = AuthorizeUpdate(dpr, parametersAsSupplierAssociatedEntityUpdateParameters);
                if (authError.HasValue)
                    return authError.Value;
            }

            var snapshot = dpr.Snapshot();

            var result = AssignOversightDate(dpr, parameters);

            if (result.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEventWithSnapshot<DataProcessingRegistration, DprSnapshot>(dpr, snapshot.FromNullable()));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return result;
        }

        public Result<DataProcessingRegistrationOversightDate, OperationError> UpdateOversightDate(Guid dataProcessingRegistrationUuid, Guid oversightDateUuid, UpdatedDataProcessingRegistrationOversightDateParameters parameters)
        {
            return GetByUuid(dataProcessingRegistrationUuid)
                .Bind(dpr => dpr.GetOversightDate(oversightDateUuid)
                    .Select(oversightDate => (dpr, oversightDate)))
                .Bind(tuple => Update(() => tuple.dpr, PerformOversightDateUpdates, (tuple.oversightDate.Id, parameters))
                    .Select(_ => tuple.oversightDate));
        }

        public Maybe<OperationError> DeleteOversightDate(Guid dataProcessingRegistrationUuid, Guid oversightDateUuid)
        {
            return GetByUuid(dataProcessingRegistrationUuid)
                .Bind(dpr => dpr.GetOversightDate(oversightDateUuid)
                    .Select(oversightDate => (dpr, oversightDate)))
                .Bind(tuple => PerformRemoveOversightDate(tuple.dpr, tuple.oversightDate))
                .Match(_ => Maybe<OperationError>.None, error => error);
        }


        public Result<DataProcessingRegistration, OperationError> AddRole(Guid dprUuid, UserRolePair assignment)
        {
            return AddRoles(dprUuid, assignment.WrapAsEnumerable());
        }

        public Result<DataProcessingRegistration, OperationError> RemoveRole(Guid dprUuid, UserRolePair assignment)
        {
            return GetByUuidAndAuthorizeRead(dprUuid)
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

        private Result<DataProcessingRegistration, OperationError> Update<TParameters>(
            Func<Result<DataProcessingRegistration, OperationError>> getDpr,
            Func<DataProcessingRegistration, TParameters, Result<DataProcessingRegistration, OperationError>>
                performUpdates, TParameters parameters)
        {
            using var transaction = _transactionManager.Begin();

            var dprResult = getDpr();

            if (dprResult.Failed)
            {
                return dprResult.Error;
            }

            var dpr = dprResult.Value;

            if (parameters is ISupplierAssociatedEntityUpdateParameters parametersAsSupplierAssociatedEntityUpdateParameters)
            {
                var authError = AuthorizeUpdate(dpr, parametersAsSupplierAssociatedEntityUpdateParameters);
                if (authError.HasValue)
                    return authError.Value;
            }

            var snapshot = dpr.Snapshot();

            var result = performUpdates(dpr, parameters);

            if (result.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEventWithSnapshot<DataProcessingRegistration, DprSnapshot>(result.Value, snapshot.FromNullable()));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return result;
        }

        private Maybe<OperationError> AuthorizeUpdate(DataProcessingRegistration dpr,
            ISupplierAssociatedEntityUpdateParameters parametersAsSupplierAssociatedEntityUpdateParameters)
        {
            var authorizationModel = _authorizationContext.GetAuthorizationModel(dpr);
            var authorizeUpdateResult = authorizationModel.AuthorizeUpdate(dpr, parametersAsSupplierAssociatedEntityUpdateParameters);
            if (authorizeUpdateResult.Failed) return authorizeUpdateResult.Error;
            var isAuthorized = authorizeUpdateResult.Value;
            if (!isAuthorized)
            {
                return new OperationError($"User is unauthorized to update Data Processing Registration with uuid: {dpr.Uuid}", OperationFailure.Forbidden);
            }

            return Maybe<OperationError>.None;
        }

        private Result<DataProcessingRegistration, OperationError> PerformUpdates<TParameters>(DataProcessingRegistration dpr, TParameters parameters) where TParameters : BaseDataProcessingRegistrationParameters
        {
            //Optionally apply changes across the entire update specification
            return dpr
                .WithOptionalUpdate(parameters.Name, (registration, changedName) => UpdateName(registration.Id, changedName))
                .Bind(registration => registration.WithOptionalUpdate(parameters.General, UpdateGeneralData))
                .Bind(registration => registration.WithOptionalUpdate(parameters.SystemUsageUuids, UpdateSystemUsageAssignments))
                .Bind(registration => registration.WithOptionalUpdate(parameters.Oversight, UpdateOversightData))
                .Bind(registration => registration.WithOptionalUpdate(parameters.Roles, UpdateRolesData))
                .Bind(registration => registration.WithOptionalUpdate(parameters.ExternalReferences, PerformReferencesUpdate))
                ;
        }

        private Result<DataProcessingRegistration, OperationError> UpdateName(int id, string name)
        {
            return Modify
            (
                id,
                registration =>
                    _namingService
                        .ChangeName(registration, name)
                        .Match
                        (
                            error => error,
                            () => Result<DataProcessingRegistration, OperationError>.Success(registration)
                        )
            );
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
                        var assignmentResult = AssignRole(dpr.Id, roleId.Value, userId.Value);
                        if (assignmentResult.Failed)
                            return new OperationError($"Failed to assign role with Uuid: {item.RoleUuid} from user with Uuid: {item.UserUuid}, with following error message: {assignmentResult.Error.Message.GetValueOrEmptyString()}", assignmentResult.Error.FailureType);
                        break;

                    case EnumerableExtensions.EnumerableDelta.Removed:
                        var removeResult = RemoveRole(dpr.Id, roleId.Value, userId.Value);
                        if (removeResult.Failed)
                            return new OperationError($"Failed to remove role with Uuid: {item.RoleUuid} from user with Uuid: {item.UserUuid}, with following error message: {removeResult.Error.Message.GetValueOrEmptyString()}", removeResult.Error.FailureType);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return dpr;
        }

        private Result<DataProcessingRegistrationRight, OperationError> AssignRole(int id, int roleId, int userId)
        {
            return Modify(id, registration => _roleAssignmentsService.AssignRole(registration, roleId, userId));
        }

        private Result<DataProcessingRegistrationRight, OperationError> RemoveRole(int id, int roleId, int userId)
        {
            return Modify(id, registration => _roleAssignmentsService.RemoveRole(registration, roleId, userId));
        }

        private Result<DataProcessingRegistration, OperationError> UpdateOversightData(DataProcessingRegistration dpr, UpdatedDataProcessingRegistrationOversightDataParameters parameters)
        {
            return dpr
                .WithOptionalUpdate(parameters.OversightOptionUuids, UpdateOversightOptions)
                .Bind(r => r.WithOptionalUpdate(parameters.OversightOptionsRemark, (registration, remark) => UpdateOversightOptionRemark(registration.Id, remark)))
                .Bind(r => r.WithOptionalUpdate(parameters.OversightInterval, (registration, interval) => UpdateOversightInterval(registration.Id, interval ?? YearMonthIntervalOption.Undecided)))
                .Bind(r => r.WithOptionalUpdate(parameters.OversightIntervalRemark, (registration, remark) => UpdateOversightIntervalRemark(registration.Id, remark)))
                .Bind(r => r.WithOptionalUpdate(parameters.IsOversightCompleted, (registration, completed) => UpdateIsOversightCompleted(registration.Id, completed ?? YesNoUndecidedOption.Undecided)))
                .Bind(r => r.WithOptionalUpdate(parameters.OversightCompletedRemark, (registration, remark) => UpdateOversightCompletedRemark(registration.Id, remark)))
                .Bind(r => r.WithOptionalUpdate(parameters.OversightScheduledInspectionDate, (registration, date) => UpdateOversightScheduledInspectionDate(registration.Id, date)))
                .Bind(r => r.WithOptionalUpdate(parameters.OversightDates, UpdateOversightDates));
        }
        public Result<DataProcessingRegistration, OperationError> UpdateOversightCompletedRemark(int id, string remark)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.OversightCompletedRemark = remark;
                return registration;
            });
        }

        private Result<DataProcessingRegistration, OperationError> UpdateIsOversightCompleted(int id, YesNoUndecidedOption isOversightCompleted)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                var oversightDates = registration.SetOversightCompleted(isOversightCompleted);
                if (oversightDates.HasValue)
                {
                    oversightDates.Value.ToList().ForEach(_dataProcessingRegistrationOversightDateRepository.Delete);
                }
                return registration;
            });
        }

        private Result<DataProcessingRegistration, OperationError> UpdateOversightScheduledInspectionDate(int id, DateTime? oversightScheduledInspectionDate)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.SetOversightScheduledInspectionDate(oversightScheduledInspectionDate);

                return registration;
            });
        }

        private Result<DataProcessingRegistration, OperationError> UpdateOversightOptionRemark(int id, string remark)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.OversightOptionRemark = remark;
                return registration;
            });
        }

        private Result<DataProcessingRegistration, OperationError> UpdateOversightInterval(int id, YearMonthIntervalOption oversightInterval)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.OversightInterval = oversightInterval;
                return registration;
            });
        }

        private Result<DataProcessingRegistration, OperationError> UpdateOversightIntervalRemark(int id, string remark)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.OversightIntervalRemark = remark;
                return registration;
            });
        }

        private Maybe<OperationError> UpdateOversightDates(DataProcessingRegistration dpr, Maybe<IEnumerable<UpdatedDataProcessingRegistrationOversightDate>> oversightDates)
        {
            // As this is "dumb" data (i.e. we don't know if two with equal data is supposed to be the same or two different registrations) we first remove all before assigning new values.
            var oldDates = dpr.OversightDates.ToList();
            foreach (var oldDate in oldDates)
            {
                var removeResult = PerformRemoveOversightDate(dpr, oldDate);

                if (removeResult.Failed)
                    return new OperationError($"Failed to remove old oversight date with Id: {oldDate.Id}. Error message: {removeResult.Error.Message.GetValueOrEmptyString()}", removeResult.Error.FailureType);
            }

            if (oversightDates.IsNone)
                return Maybe<OperationError>.None;

            foreach (var newDate in oversightDates.Value)
            {
                var oversightOptionIdResult = ResolveOversightOptionId(newDate.OversightOptionUuid);
                if (oversightOptionIdResult.Failed)
                    return oversightOptionIdResult.Error;

                if (!newDate.CompletedAt.HasValue)
                    return new OperationError("CompletedAt must be provided when assigning oversight dates", OperationFailure.BadInput);

                var assignResult = dpr.AssignOversightDate(newDate.CompletedAt.Value, newDate.Remark, newDate.OversightReportLink, newDate.OversightReportLinkName, oversightOptionIdResult.Value);

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
                (registration, oversightOptionId) => AssignOversightOption(registration.Id, oversightOptionId).MatchFailure(),
                (registration, oversightOption) => RemoveOversightOption(registration.Id, oversightOption.Id).MatchFailure()
            );
        }

        private Result<DataProcessingOversightOption, OperationError> AssignOversightOption(int id, int oversightOptionId)
        {
            return Modify(id, registration => _oversightOptionAssignmentService.Assign(registration, oversightOptionId));
        }

        private Result<DataProcessingOversightOption, OperationError> RemoveOversightOption(int id, int oversightOptionId)
        {
            return Modify(id, registration => _oversightOptionAssignmentService.Remove(registration, oversightOptionId));
        }

        private Result<DataProcessingRegistration, OperationError> UpdateGeneralData(DataProcessingRegistration dpr, UpdatedDataProcessingRegistrationGeneralDataParameters parameters)
        {
            return dpr
                .WithOptionalUpdate(parameters.DataResponsibleUuid, UpdateDataResponsible)
                .Bind(r => r.WithOptionalUpdate(parameters.DataResponsibleRemark, (registration, remark) => UpdateDataResponsibleRemark(registration.Id, remark)))
                .Bind(r => r.WithOptionalUpdate(parameters.IsAgreementConcluded, (registration, concluded) => UpdateIsAgreementConcluded(registration.Id, concluded ?? YesNoIrrelevantOption.UNDECIDED)))
                .Bind(r => r.WithOptionalUpdate(parameters.IsAgreementConcludedRemark, (registration, concludedRemark) => UpdateAgreementConcludedRemark(registration.Id, concludedRemark)))
                .Bind(r => r.WithOptionalUpdate(parameters.AgreementConcludedAt, (registration, concludedAt) => UpdateAgreementConcludedAt(registration.Id, concludedAt)))
                .Bind(r => r.WithOptionalUpdate(parameters.BasisForTransferUuid, UpdateBasisForTransfer))
                .Bind(r => r.WithOptionalUpdate(parameters.TransferToInsecureThirdCountries, (registration, newValue) => UpdateTransferToInsecureThirdCountries(registration.Id, newValue ?? YesNoUndecidedOption.Undecided)))
                .Bind(r => r.WithOptionalUpdate(parameters.InsecureCountriesSubjectToDataTransferUuids, UpdateInsecureCountriesSubjectToDataTransfer))
                .Bind(r => r.WithOptionalUpdate(parameters.DataProcessorUuids, UpdateDataProcessors))
                .Bind(r => r.WithOptionalUpdate(parameters.HasSubDataProcessors, (registration, newValue) => SetSubDataProcessorsState(registration.Id, newValue ?? YesNoUndecidedOption.Undecided)))
                .Bind(r => r.WithOptionalUpdate(parameters.SubDataProcessors, UpdateSubDataProcessors))
                .Bind(r => r.WithOptionalUpdate(parameters.MainContractUuid, UpdateMainContract))
                .Bind(r => r.WithOptionalUpdate(parameters.EnforceInvalidity, (registration, newValue) => registration.EnforceInvalidity = newValue))
                .Bind(r => r.WithOptionalUpdate(parameters.ResponsibleUnitUuid, UpdateResponsibleUnit));
        }

        private Result<DataProcessingRegistration, OperationError> SetSubDataProcessorsState(int id, YesNoUndecidedOption state)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                var result = registration.SetHasSubDataProcessors(state);
                var removedSubDataProcessors = result.RemovedSubDataProcessors.ToList();
                _sdpRepository.RemoveRange(removedSubDataProcessors);
                return registration;
            });
        }

        private Result<DataProcessingRegistration, OperationError> UpdateIsAgreementConcluded(int id, YesNoIrrelevantOption concluded)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.SetIsAgreementConcluded(concluded);
                return registration;
            });
        }

        private Result<DataProcessingRegistration, OperationError> UpdateAgreementConcludedAt(int id, DateTime? concludedAtDate)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.AgreementConcludedAt = concludedAtDate;
                return registration;
            });
        }

        private Result<DataProcessingRegistration, OperationError> UpdateAgreementConcludedRemark(int id, string remark)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.AgreementConcludedRemark = remark;
                return registration;
            });
        }

        private Result<DataProcessingRegistration, OperationError> UpdateTransferToInsecureThirdCountries(int id, YesNoUndecidedOption transferToInsecureThirdCountries)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.SetTransferToInsecureThirdCountries(transferToInsecureThirdCountries);
                return registration;
            });
        }

        private Result<DataProcessingRegistration, OperationError> UpdateDataResponsibleRemark(int id, string remark)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.DataResponsibleRemark = remark;
                return registration;
            });
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

        private Result<DataProcessingRegistration, OperationError> UpdateSystemUsageAssignments(DataProcessingRegistration dpr, IEnumerable<Guid> systemUsageUuids)
        {
            return _assignmentUpdateService.UpdateUniqueMultiAssignment
            (
                "system usage",
                dpr,
                systemUsageUuids.FromNullable(),
                usageUuid => _entityIdentityResolver.ResolveDbId<ItSystemUsage>(usageUuid).Match<Result<int, OperationError>>(optionId => optionId, () => new OperationError($"Failed to resolve Id for Uuid {usageUuid}", OperationFailure.BadInput)),
                registration => registration.SystemUsages.ToList(),
                (registration, usageId) => AssignSystem(registration.Id, usageId).MatchFailure(),
                (registration, usage) => RemoveSystem(registration.Id, usage.Id).MatchFailure()
            ).Match<Result<DataProcessingRegistration, OperationError>>(error => error, () => dpr);
        }

        private Result<ItSystemUsage, OperationError> AssignSystem(int id, int systemId)
        {
            return Modify(id, registration => _systemAssignmentService.AssignSystem(registration, systemId));
        }

        private Result<ItSystemUsage, OperationError> RemoveSystem(int id, int systemId)
        {
            return Modify(id, registration => _systemAssignmentService.RemoveSystem(registration, systemId));
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
                (registration, subDataProcessorId) => AssignSubDataProcessor(registration.Id, subDataProcessorId, detailsLookup[orgIdLookup[subDataProcessorId]]).MatchFailure(),
                (registration, subDataProcessor) => RemoveSubDataProcessor(registration.Id, subDataProcessor.Id).MatchFailure(),
                update: (registration, subDataProcessor) => UpdateSubDataProcessor(registration.Id, subDataProcessor.Id, detailsLookup[subDataProcessor.Uuid]).MatchFailure()
                );
        }

        private Result<SubDataProcessor, OperationError> UpdateSubDataProcessor(int id, int organizationId, SubDataProcessorDetailsParameters details)
        {
            if (details == null)
            {
                throw new ArgumentNullException(nameof(details));
            }

            return Modify(id, registration => _dataProcessingRegistrationDataProcessorAssignmentService.UpdateSubDataProcessor(registration, organizationId, details.BasisForTransferOptionId, details.InsecureCountryParameters.Transfer, details.InsecureCountryParameters.InsecureCountryOptionId));
        }

        private Result<SubDataProcessor, OperationError> AssignSubDataProcessor(int id, int organizationId, Maybe<SubDataProcessorDetailsParameters> details)
        {
            return Modify(id, registration => _dataProcessingRegistrationDataProcessorAssignmentService
                .AssignSubDataProcessor(registration, organizationId)
                .Bind
                (
                    dpr =>
                    {
                        return details.Match
                        (
                            parameters => UpdateSubDataProcessor(id, organizationId, parameters),
                            () => dpr
                        );
                    })
            );
        }

        private Result<SubDataProcessor, OperationError> RemoveSubDataProcessor(int id, int organizationId)
        {
            return Modify(id, registration =>
            {
                var result = _dataProcessingRegistrationDataProcessorAssignmentService.RemoveSubDataProcessor(registration, organizationId);
                if (result.Ok)
                {
                    _sdpRepository.Delete(result.Value);
                }
                return result;
            });
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
                (registration, dataProcessorId) => AssignDataProcessor(registration.Id, dataProcessorId).MatchFailure(),
                (registration, dataProcessor) => RemoveDataProcessor(registration.Id, dataProcessor.Id).MatchFailure()
            );
        }
        private Result<Organization, OperationError> AssignDataProcessor(int id, int organizationId)
        {
            return Modify(id, registration => _dataProcessingRegistrationDataProcessorAssignmentService.AssignDataProcessor(registration, organizationId));
        }

        private Result<Organization, OperationError> RemoveDataProcessor(int id, int organizationId)
        {
            return Modify(id, registration => _dataProcessingRegistrationDataProcessorAssignmentService.RemoveDataProcessor(registration, organizationId));
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
                (registration, countryOptionId) => AssignInsecureThirdCountry(registration.Id, countryOptionId).MatchFailure(),
                (registration, countryOption) => RemoveInsecureThirdCountry(registration.Id, countryOption.Id).MatchFailure()
            );
        }

        private Result<DataProcessingCountryOption, OperationError> AssignInsecureThirdCountry(int id, int countryId)
        {
            return Modify(id, registration => _countryAssignmentService.Assign(registration, countryId));
        }

        private Result<DataProcessingCountryOption, OperationError> RemoveInsecureThirdCountry(int id, int countryId)
        {
            return Modify(id, registration => _countryAssignmentService.Remove(registration, countryId));
        }

        private Maybe<OperationError> UpdateBasisForTransfer(DataProcessingRegistration dpr, Guid? basisForTransferUuid)
        {
            if (!basisForTransferUuid.HasValue)
                return ClearBasisForTransfer(dpr.Id)
                    .Match(
                        _ => Maybe<OperationError>.None,
                        error => error.FailureType == OperationFailure.BadState ? Maybe<OperationError>.None : error
                    );

            var dbId = _entityIdentityResolver.ResolveDbId<DataProcessingBasisForTransferOption>(basisForTransferUuid.Value);

            if (dbId.IsNone)
                return new OperationError($"Basis for transfer option with uuid {basisForTransferUuid.Value} could not be found", OperationFailure.BadInput);

            return AssignBasisForTransfer(dpr.Id, dbId.Value)
                .MatchFailure();
        }

        private Result<DataProcessingBasisForTransferOption, OperationError> AssignBasisForTransfer(int id, int basisForTransferId)
        {
            return Modify(id, registration => _basisForTransferAssignmentService.Assign(registration, basisForTransferId));
        }

        private Result<DataProcessingBasisForTransferOption, OperationError> ClearBasisForTransfer(int id)
        {
            return Modify(id, registration => _basisForTransferAssignmentService.Clear(registration));
        }

        private Result<DataProcessingRegistration, OperationError> UpdateMainContract(DataProcessingRegistration dpr, Guid? contractUuid)
        {
            if (contractUuid.HasValue)
            {
                return _entityIdentityResolver.ResolveDbId<ItContract>(contractUuid.Value)
                    .Match
                    (
                        contractId => UpdateMainContract(dpr.Id, contractId),
                        () => new OperationError($"It contract with uuid {contractUuid.Value} could not be found", OperationFailure.BadInput)
                    );
            }
            return RemoveMainContract(dpr.Id);
        }

        private Result<DataProcessingRegistration, OperationError> UpdateMainContract(int id, int contractId)
        {
            return Modify(id, registration => registration.AssignMainContract(contractId));
        }

        private Result<DataProcessingRegistration, OperationError> RemoveMainContract(int id)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.ResetMainContract();
                return registration;
            });
        }

        private Maybe<OperationError> UpdateDataResponsible(DataProcessingRegistration dpr, Guid? dataResponsibleUuid)
        {
            if (!dataResponsibleUuid.HasValue)
                return ClearDataResponsible(dpr.Id)
                    .Match(
                        _ => Maybe<OperationError>.None,
                        error => error.FailureType == OperationFailure.BadState ? Maybe<OperationError>.None : error
                    );

            var dbId = _entityIdentityResolver.ResolveDbId<DataProcessingDataResponsibleOption>(dataResponsibleUuid.Value);

            if (dbId.IsNone)
                return new OperationError($"Data responsible option with uuid {dataResponsibleUuid.Value} could not be found", OperationFailure.BadInput);

            return AssignDataResponsible(dpr.Id, dbId.Value)
                .MatchFailure();
        }
        
        public Result<DataProcessingDataResponsibleOption, OperationError> AssignDataResponsible(int id, int dataResponsibleId)
        {
            return Modify(id, registration => _dataResponsibleAssignmentService.Assign(registration, dataResponsibleId));
        }

        public Result<DataProcessingDataResponsibleOption, OperationError> ClearDataResponsible(int id)
        {
            return Modify(id, registration => _dataResponsibleAssignmentService.Clear(registration));
        }

        private Result<TSuccess, OperationError> Modify<TSuccess>(int id, Func<DataProcessingRegistration, Result<TSuccess, OperationError>> mutation)
        {
            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var registration = result.Value;

            var mutationResult = mutation(registration);

            if (mutationResult.Ok)
            {
                _repository.Update(registration);
            }

            return mutationResult;
        }


        public Maybe<OperationError> Delete(Guid dataProcessingRegistrationUuid)
        {
            var dbId = _entityIdentityResolver.ResolveDbId<DataProcessingRegistration>(dataProcessingRegistrationUuid);

            if (dbId.IsNone)
                return new OperationError(OperationFailure.NotFound);

            return Delete(dbId.Value)
                .Match(_ => Maybe<OperationError>.None, error => error);
        }

        public Result<DataProcessingRegistration, OperationError> Delete(int id)
        {
            using var transaction = _transactionManager.Begin();

            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var registrationToDelete = result.Value;

            if (!_authorizationContext.AllowDelete(registrationToDelete))
                return new OperationError(OperationFailure.Forbidden);

            _repository.DeleteById(id);
            transaction.Commit();
            return registrationToDelete;
        }

        private Result<DataProcessingRegistration, OperationError> PerformOversightDateUpdates(
            DataProcessingRegistration registration, (int id, UpdatedDataProcessingRegistrationOversightDateParameters parameters) methodParameters)
        {
            var parameters = methodParameters.parameters;

            var authorizeError = AuthorizeUpdate(registration, parameters);
            if (authorizeError.HasValue) return authorizeError.Value;

            var oversightDateId = methodParameters.id;
            return registration.WithOptionalUpdate(parameters.CompletedAt,

                    (dpr, changedDate) => dpr.ModifyOversightDateDate(oversightDateId, changedDate))
                .Bind(updateRegistration => updateRegistration.WithOptionalUpdate(parameters.Remark,
                    (dpr, changedRemark) => dpr.ModifyOversightDateRemark(oversightDateId, changedRemark)))
                .Bind(updateRegistration => updateRegistration.WithOptionalUpdate(parameters.OversightReportLink,
                    (dpr, changedLink) => dpr.ModifyOversightDateReportLink(oversightDateId, changedLink)))
                .Bind(updateRegistration => updateRegistration.WithOptionalUpdate(parameters.OversightReportLinkName,
                    (dpr, changedLinkName) => dpr.ModifyOversightDateReportLinkName(oversightDateId, changedLinkName)))
                .Bind(updateRegistration =>
                {
                    if (!parameters.OversightOptionUuid.HasChange)
                        return updateRegistration;

                    return ResolveOversightOptionId(parameters.OversightOptionUuid.NewValue)
                        .Bind(changedOptionId => updateRegistration.ModifyOversightDateOptionId(oversightDateId, changedOptionId))
                        .Select(_ => updateRegistration);
                });
        }

        private Result<DataProcessingRegistrationOversightDate, OperationError> AssignOversightDate(DataProcessingRegistration dpr, UpdatedDataProcessingRegistrationOversightDateParameters parameters)
        {
            var remark = parameters.Remark.HasChange ? parameters.Remark.NewValue : null;
            var oversightReportLink = parameters.OversightReportLink.HasChange ? parameters.OversightReportLink.NewValue : null;
            var oversightReportLinkName = parameters.OversightReportLinkName.HasChange ? parameters.OversightReportLinkName.NewValue : null;
            var oversightOptionUuid = parameters.OversightOptionUuid.HasChange ? parameters.OversightOptionUuid.NewValue : null;
            return ResolveOversightOptionId(oversightOptionUuid)
                .Bind(oversightOptionId =>
                {
                    if (!parameters.CompletedAt.HasChange || !parameters.CompletedAt.NewValue.HasValue)
                        return new OperationError("CompletedAt must be provided when creating an oversight date", OperationFailure.BadInput);

                    return dpr.AssignOversightDate(parameters.CompletedAt.NewValue.Value, remark, oversightReportLink, oversightReportLinkName, oversightOptionId);
                });
        }

        private Result<int?, OperationError> ResolveOversightOptionId(Guid? oversightOptionUuid)
        {
            if (!oversightOptionUuid.HasValue)
                return (int?)null;

            var oversightOptionId = _entityIdentityResolver.ResolveDbId<DataProcessingOversightOption>(oversightOptionUuid.Value);
            if (oversightOptionId.IsNone)
                return new OperationError($"Failed to resolve Id for Uuid {oversightOptionUuid.Value}", OperationFailure.BadInput);

            return oversightOptionId.Value;
        }

        private Result<DataProcessingRegistrationOversightDate, OperationError> PerformRemoveOversightDate(DataProcessingRegistration registration, DataProcessingRegistrationOversightDate oversightDate)
        {
            using var transaction = _transactionManager.Begin();

            var authorizationModel = _authorizationContext.GetAuthorizationModel(registration);
            var authorizeUpdate = authorizationModel.AuthorizeChildEntityDelete(registration, oversightDate);
            if (!authorizeUpdate)
            {
                return new OperationError($"User is unauthorized to update Data Processing Registration with uuid: {registration.Uuid}", OperationFailure.Forbidden);
            }

            var removedRegistration = registration.RemoveOversightDate(oversightDate.Id);
            if (removedRegistration.Ok)
            {
                _oversightDateRepository.Delete(removedRegistration.Value);
                _oversightDateRepository.Save();

                _domainEvents.Raise(new EntityUpdatedEvent<DataProcessingRegistration>(registration));

                transaction.Commit();
            }
            return removedRegistration;
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

        private Result<DataProcessingRegistration, OperationError> AddRoles(Guid dprUuid,
            IEnumerable<UserRolePair> assignments)
        {
            return GetByUuidAndAuthorizeRead(dprUuid)
                .Bind(dpr => GetRoleAssignmentUpdates(dpr, assignments))
                .Bind(update => Update(dprUuid, update));
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
