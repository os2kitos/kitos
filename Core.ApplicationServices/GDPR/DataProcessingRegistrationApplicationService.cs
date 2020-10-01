﻿using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Shared;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.GDPR;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Reference;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.GDPR
{
    public class DataProcessingRegistrationApplicationService : IDataProcessingRegistrationApplicationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IDataProcessingRegistrationRepository _repository;
        private readonly IDataProcessingRegistrationNamingService _namingService;
        private readonly IDataProcessingRegistrationRoleAssignmentsService _roleAssignmentsService;
        private readonly IReferenceRepository _referenceRepository;
        private readonly IDataProcessingRegistrationSystemAssignmentService _systemAssignmentService;
        private readonly IDataProcessingRegistrationDataProcessorAssignmentService _dataProcessingRegistrationDataProcessorAssignmentService;
        private readonly IDataProcessingRegistrationInsecureCountriesAssignmentService _countryAssignmentService;
        private readonly IDataProcessingRegistrationBasisForTransferAssignmentService _basisForTransferAssignmentService;
        private readonly ITransactionManager _transactionManager;
        private readonly IGenericRepository<DataProcessingRegistrationRight> _rightRepository;

        public DataProcessingRegistrationApplicationService(
            IAuthorizationContext authorizationContext,
            IDataProcessingRegistrationRepository repository,
            IDataProcessingRegistrationNamingService namingService,
            IDataProcessingRegistrationRoleAssignmentsService roleAssignmentsService,
            IReferenceRepository referenceRepository,
            IDataProcessingRegistrationSystemAssignmentService systemAssignmentService,
            IDataProcessingRegistrationDataProcessorAssignmentService dataProcessingRegistrationDataProcessorAssignmentService,
            IDataProcessingRegistrationInsecureCountriesAssignmentService countryAssignmentService,
            IDataProcessingRegistrationBasisForTransferAssignmentService basisForTransferAssignmentService,
            ITransactionManager transactionManager,
            IGenericRepository<DataProcessingRegistrationRight> rightRepository)
        {
            _authorizationContext = authorizationContext;
            _repository = repository;
            _namingService = namingService;
            _roleAssignmentsService = roleAssignmentsService;
            _referenceRepository = referenceRepository;
            _systemAssignmentService = systemAssignmentService;
            _dataProcessingRegistrationDataProcessorAssignmentService = dataProcessingRegistrationDataProcessorAssignmentService;
            _countryAssignmentService = countryAssignmentService;
            _basisForTransferAssignmentService = basisForTransferAssignmentService;
            _transactionManager = transactionManager;
            _rightRepository = rightRepository;
        }

        public Result<DataProcessingRegistration, OperationError> Create(int organizationId, string name)
        {
            if (!_authorizationContext.AllowCreate<DataProcessingRegistration>(organizationId))
                return new OperationError(OperationFailure.Forbidden);

            using var transaction = _transactionManager.Begin(IsolationLevel.Serializable);
            var error = _namingService.ValidateSuggestedNewRegistrationName(organizationId, name);

            if (error.HasValue)
                return error.Value;

            var registration = new DataProcessingRegistration
            {
                OrganizationId = organizationId,
                Name = name
            };

            var dataProcessingRegistration = _repository.Add(registration);
            transaction.Commit();
            return dataProcessingRegistration;
        }

        public Maybe<OperationError> ValidateSuggestedNewRegistrationName(int organizationId, string name)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            return _namingService.ValidateSuggestedNewRegistrationName(organizationId, name);
        }

        public Result<DataProcessingRegistration, OperationError> Delete(int id)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.Serializable);

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

        public Result<DataProcessingRegistration, OperationError> Get(int id)
        {
            return WithReadAccess<DataProcessingRegistration>(id, result => result);
        }

        public Result<IQueryable<DataProcessingRegistration>, OperationError> GetOrganizationData(int organizationId, int skip, int take)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            if (take < 1 || skip < 0 || take > PagingContraints.MaxPageSize)
                return new OperationError("Invalid paging arguments", OperationFailure.BadInput);

            var registrations = _repository
                .Search(organizationId, Maybe<string>.None)
                .OrderBy(x => x.Id)
                .Skip(skip)
                .Take(take);

            return Result<IQueryable<DataProcessingRegistration>, OperationError>.Success(registrations);
        }

        public Result<DataProcessingRegistration, OperationError> UpdateName(int id, string name)
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

        public Result<ExternalReference, OperationError> SetMasterReference(int id, int referenceId)
        {
            return Modify(id, registration =>
                _referenceRepository
                    .Get(referenceId)
                    .Select(registration.SetMasterReference)
                    .Match(result => result, () => new OperationError("Invalid reference Id", OperationFailure.BadInput)));
        }

        public Result<(DataProcessingRegistration registration, IEnumerable<DataProcessingRegistrationRole> roles), OperationError> GetAvailableRoles(int id)
        {
            return WithReadAccess<(DataProcessingRegistration registration, IEnumerable<DataProcessingRegistrationRole> roles)>(
                id,
                registration => (registration, _roleAssignmentsService.GetApplicableRoles(registration).ToList()));
        }

        public Result<IEnumerable<User>, OperationError> GetUsersWhichCanBeAssignedToRole(int id, int roleId, string nameEmailQuery, int pageSize)
        {
            if (pageSize < 1)
                throw new ArgumentException($"{nameof(pageSize)} must be above 0");

            return WithReadAccess(id, registration =>
            {
                return _roleAssignmentsService
                    .GetUsersWhichCanBeAssignedToRole(registration, roleId, nameEmailQuery.FromNullable())
                    .Select<IEnumerable<User>>(users =>
                        users
                            .OrderBy(x => x.Id)
                            .Take(pageSize)
                            .OrderBy(x => x.Name)
                            .ToList()
                    );
            });
        }

        public Result<DataProcessingRegistrationRight, OperationError> AssignRole(int id, int roleId, int userId)
        {
            return Modify(id, registration => _roleAssignmentsService.AssignRole(registration, roleId, userId));
        }

        public Result<DataProcessingRegistrationRight, OperationError> RemoveRole(int id, int roleId, int userId)
        {
            return Modify(id, registration =>
            {
                var removeResult = _roleAssignmentsService.RemoveRole(registration, roleId, userId);

                if (removeResult.Ok)
                {
                    _rightRepository.Delete(removeResult.Value);
                }

                return removeResult;
            });
        }

        public Result<IEnumerable<ItSystem>, OperationError> GetSystemsWhichCanBeAssigned(int id, string nameQuery, int pageSize)
        {
            if (string.IsNullOrEmpty(nameQuery)) throw new ArgumentException($"{nameof(nameQuery)} must be defined");
            if (pageSize < 1) throw new ArgumentException($"{nameof(pageSize)} must be above 0");

            return WithReadAccess<IEnumerable<ItSystem>>(id, registration =>
                _systemAssignmentService
                    .GetApplicableSystems(registration)
                    .ByPartOfName(nameQuery)
                    .OrderBy(x => x.Id)
                    .Take(pageSize)
                    .OrderBy(x => x.Name)
                    .ToList()
            );
        }

        public Result<ItSystem, OperationError> AssignSystem(int id, int systemId)
        {
            return Modify(id, registration => _systemAssignmentService.AssignSystem(registration, systemId));
        }

        public Result<ItSystem, OperationError> RemoveSystem(int id, int systemId)
        {
            return Modify(id, registration => _systemAssignmentService.RemoveSystem(registration, systemId));
        }

        public Result<IEnumerable<Organization>, OperationError> GetDataProcessorsWhichCanBeAssigned(int id, string nameQuery, int pageSize)
        {
            if (string.IsNullOrEmpty(nameQuery)) throw new ArgumentException($"{nameof(nameQuery)} must be defined");
            if (pageSize < 1) throw new ArgumentException($"{nameof(pageSize)} must be above 0");

            return WithReadAccess<IEnumerable<Organization>>(id,
                registration =>
                    _dataProcessingRegistrationDataProcessorAssignmentService
                        .GetApplicableDataProcessors(registration)
                        .ByPartOfName(nameQuery)
                        .OrderBy(x => x.Id)
                        .Take(pageSize)
                        .OrderBy(x => x.Name)
                        .ToList());
        }

        public Result<Organization, OperationError> AssignDataProcessor(int id, int organizationId)
        {
            return Modify(id, registration => _dataProcessingRegistrationDataProcessorAssignmentService.AssignDataProcessor(registration, organizationId));
        }

        public Result<Organization, OperationError> RemoveDataProcessor(int id, int organizationId)
        {
            return Modify(id, registration => _dataProcessingRegistrationDataProcessorAssignmentService.RemoveDataProcessor(registration, organizationId));
        }

        public Result<IEnumerable<Organization>, OperationError> GetSubDataProcessorsWhichCanBeAssigned(int id, string nameQuery, int pageSize)
        {
            if (string.IsNullOrEmpty(nameQuery)) throw new ArgumentException($"{nameof(nameQuery)} must be defined");
            if (pageSize < 1) throw new ArgumentException($"{nameof(pageSize)} must be above 0");

            return WithReadAccess<IEnumerable<Organization>>(id,
                registration =>
                    _dataProcessingRegistrationDataProcessorAssignmentService
                        .GetApplicableSubDataProcessors(registration)
                        .ByPartOfName(nameQuery)
                        .OrderBy(x => x.Id)
                        .Take(pageSize)
                        .OrderBy(x => x.Name)
                        .ToList());
        }

        public Result<DataProcessingRegistration, OperationError> SetSubDataProcessorsState(int id, YesNoUndecidedOption state)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.HasSubDataProcessors = state;
                return registration;
            });
        }

        public Result<Organization, OperationError> AssignSubDataProcessor(int id, int organizationId)
        {
            return Modify(id, registration => _dataProcessingRegistrationDataProcessorAssignmentService.AssignSubDataProcessor(registration, organizationId));
        }

        public Result<Organization, OperationError> RemoveSubDataProcessor(int id, int organizationId)
        {
            return Modify(id, registration => _dataProcessingRegistrationDataProcessorAssignmentService.RemoveSubDataProcessor(registration, organizationId));
        }


        public Result<DataProcessingRegistration, OperationError> UpdateIsAgreementConcluded(int id, YesNoIrrelevantOption concluded)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.IsAgreementConcluded = concluded;
                return registration;
            });
        }

        public Result<DataProcessingRegistration, OperationError> UpdateAgreementConcludedAt(int id, DateTime? concludedAtDate)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.AgreementConcludedAt = concludedAtDate;
                return registration;
            });
        }

        public Result<DataProcessingRegistration, OperationError> UpdateTransferToInsecureThirdCountries(int id, YesNoUndecidedOption transferToInsecureThirdCountries)
        {
            return Modify<DataProcessingRegistration>(id, registration =>
            {
                registration.TransferToInsecureThirdCountries = transferToInsecureThirdCountries;
                return registration;
            });
        }

        public Result<DataProcessingCountryOption, OperationError> AssignInsecureThirdCountry(int id, int countryId)
        {
            return Modify(id, registration => _countryAssignmentService.Assign(registration, countryId));
        }

        public Result<DataProcessingCountryOption, OperationError> RemoveInsecureThirdCountry(int id, int countryId)
        {
            return Modify(id, registration => _countryAssignmentService.Remove(registration, countryId));
        }

        public Result<DataProcessingBasisForTransferOption, OperationError> AssignBasisForTransfer(int id, int basisForTransferId)
        {
            return Modify(id, registration => _basisForTransferAssignmentService.Assign(registration, basisForTransferId));
        }

        public Result<DataProcessingBasisForTransferOption, OperationError> ClearBasisForTransfer(int id)
        {
            return Modify(id, registration => _basisForTransferAssignmentService.Clear(registration));

        }

        private Result<TSuccess, OperationError> Modify<TSuccess>(int id, Func<DataProcessingRegistration, Result<TSuccess, OperationError>> mutation)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var registration = result.Value;

            if (!_authorizationContext.AllowModify(registration))
                return new OperationError(OperationFailure.Forbidden);

            var mutationResult = mutation(registration);

            if (mutationResult.Ok)
            {
                _repository.Update(registration);
                transaction.Commit();
            }

            return mutationResult;
        }

        private Result<TSuccess, OperationError> WithReadAccess<TSuccess>(int id, Func<DataProcessingRegistration, Result<TSuccess, OperationError>> authorizedAction)
        {
            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var registration = result.Value;

            if (!_authorizationContext.AllowReads(registration))
                return new OperationError(OperationFailure.Forbidden);

            return authorizedAction(registration);
        }
    }
}