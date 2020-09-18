using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.GDPR;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Shared;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
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

namespace Core.ApplicationServices.GDPR
{
    public class DataProcessingAgreementApplicationService : IDataProcessingAgreementApplicationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IDataProcessingAgreementRepository _repository;
        private readonly IDataProcessingAgreementNamingService _namingService;
        private readonly IDataProcessingAgreementRoleAssignmentsService _roleAssignmentsService;
        private readonly IReferenceRepository _referenceRepository;
        private readonly IDataProcessingAgreementSystemAssignmentService _systemAssignmentService;
        private readonly ITransactionManager _transactionManager;
        private readonly IGenericRepository<DataProcessingAgreementRight> _rightRepository;

        public DataProcessingAgreementApplicationService(
            IAuthorizationContext authorizationContext,
            IDataProcessingAgreementRepository repository,
            IDataProcessingAgreementNamingService namingService,
            IDataProcessingAgreementRoleAssignmentsService roleAssignmentsService,
            IReferenceRepository referenceRepository,
            IDataProcessingAgreementSystemAssignmentService systemAssignmentService,
            ITransactionManager transactionManager,
            IGenericRepository<DataProcessingAgreementRight> rightRepository)
        {
            _authorizationContext = authorizationContext;
            _repository = repository;
            _namingService = namingService;
            _roleAssignmentsService = roleAssignmentsService;
            _referenceRepository = referenceRepository;
            _systemAssignmentService = systemAssignmentService;
            _transactionManager = transactionManager;
            _rightRepository = rightRepository;
        }

        public Result<DataProcessingAgreement, OperationError> Create(int organizationId, string name)
        {
            if (!_authorizationContext.AllowCreate<DataProcessingAgreement>(organizationId))
                return new OperationError(OperationFailure.Forbidden);

            var error = _namingService.ValidateSuggestedNewAgreement(organizationId, name);

            if (error.HasValue)
                return error.Value;

            var dataProcessingAgreement = new DataProcessingAgreement
            {
                OrganizationId = organizationId,
                Name = name
            };

            return _repository.Add(dataProcessingAgreement);
        }

        public Maybe<OperationError> ValidateSuggestedNewAgreement(int organizationId, string name)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            return _namingService.ValidateSuggestedNewAgreement(organizationId, name);
        }

        public Result<DataProcessingAgreement, OperationError> Delete(int id)
        {
            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var agreementToDelete = result.Value;

            if (!_authorizationContext.AllowDelete(agreementToDelete))
                return new OperationError(OperationFailure.Forbidden);

            _repository.DeleteById(id);

            return agreementToDelete;
        }

        public Result<DataProcessingAgreement, OperationError> Get(int id)
        {
            return WithReadAccess<DataProcessingAgreement>(id, result => result);
        }

        public Result<IQueryable<DataProcessingAgreement>, OperationError> GetOrganizationData(int organizationId, int skip, int take)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            if (take < 1 || skip < 0 || take > PagingContraints.MaxPageSize)
                return new OperationError("Invalid paging arguments", OperationFailure.BadInput);

            var dataProcessingAgreements = _repository
                .Search(organizationId, Maybe<string>.None)
                .OrderBy(x => x.Id)
                .Skip(skip)
                .Take(take);

            return Result<IQueryable<DataProcessingAgreement>, OperationError>.Success(dataProcessingAgreements);
        }

        public Result<DataProcessingAgreement, OperationError> UpdateName(int id, string name)
        {
            return UpdateProperties(id, new DataProcessingAgreementPropertyChanges { NameChange = new ChangedValue<string>(name) });
        }

        public Result<ExternalReference, OperationError> SetMasterReference(int agreementId, int referenceId)
        {
            return WithWriteAccess(agreementId, agreement =>
            {
                var referenceResult = _referenceRepository.Get(referenceId);

                if (referenceResult.IsNone)
                {
                    return new OperationError("Invalid reference Id", OperationFailure.BadInput);
                }

                var setReferenceResult = agreement.SetMasterReference(referenceResult.Value);

                if (setReferenceResult.Ok)
                {
                    _repository.Update(agreement);
                }

                return setReferenceResult;
            });
        }

        public Result<(DataProcessingAgreement agreement, IEnumerable<DataProcessingAgreementRole> roles), OperationError> GetAvailableRoles(int id)
        {
            return WithReadAccess<(DataProcessingAgreement agreement, IEnumerable<DataProcessingAgreementRole> roles)>(
                id,
                agreement => (agreement, _roleAssignmentsService.GetApplicableRoles(agreement).ToList()));
        }

        public Result<IEnumerable<User>, OperationError> GetUsersWhichCanBeAssignedToRole(int id, int roleId, string nameEmailQuery, int pageSize)
        {
            if (pageSize < 1)
                throw new ArgumentException($"{nameof(pageSize)} must be above 0");

            return WithReadAccess(id, agreement =>
            {
                return _roleAssignmentsService
                    .GetUsersWhichCanBeAssignedToRole(agreement, roleId, nameEmailQuery.FromNullable())
                    .Select<IEnumerable<User>>(users =>
                        users
                            .OrderBy(x => x.Id)
                            .Take(pageSize)
                            .OrderBy(x => x.Name)
                            .ToList()
                    );
            });
        }

        public Result<DataProcessingAgreementRight, OperationError> AssignRole(int id, int roleId, int userId)
        {
            return WithWriteAccess(id, agreement =>
            {
                using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

                var assignmentResult = _roleAssignmentsService.AssignRole(agreement, roleId, userId);

                if (assignmentResult.Ok)
                {
                    _repository.Update(agreement);
                    transaction.Commit();
                }

                return assignmentResult;
            });
        }

        public Result<DataProcessingAgreementRight, OperationError> RemoveRole(int id, int roleId, int userId)
        {
            return WithWriteAccess(id, agreement =>
            {

                using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

                var removeResult = _roleAssignmentsService.RemoveRole(agreement, roleId, userId);

                if (removeResult.Ok)
                {
                    _rightRepository.Delete(removeResult.Value);
                    _repository.Update(agreement);
                    transaction.Commit();
                }

                return removeResult;
            });
        }

        public Result<IEnumerable<ItSystem>, OperationError> GetSystemsWhichCanBeAssigned(int id, string nameQuery, int pageSize)
        {
            if (string.IsNullOrEmpty(nameQuery)) throw new ArgumentException($"{nameof(nameQuery)} must be defined");
            if (pageSize < 1) throw new ArgumentException($"{nameof(pageSize)} must be above 0");

            return WithReadAccess<IEnumerable<ItSystem>>(id, dataProcessingAgreement =>
                _systemAssignmentService
                    .GetApplicableSystems(dataProcessingAgreement)
                    .Transform(systems => systems.ByPartOfName(nameQuery))
                    .OrderBy(x => x.Id)
                    .Take(pageSize)
                    .OrderBy(x => x.Name)
                    .ToList()
            );
        }

        public Result<ItSystem, OperationError> AssignSystem(int id, int systemId)
        {
            return WithWriteAccess(id, dataProcessingAgreement =>
            {
                var result = _systemAssignmentService.AssignSystem(dataProcessingAgreement, systemId);

                if (result.Ok)
                    _repository.Update(dataProcessingAgreement);

                return result;
            });
        }

        public Result<ItSystem, OperationError> RemoveSystem(int id, int systemId)
        {
            return WithWriteAccess(id, dataProcessingAgreement =>
            {
                var result = _systemAssignmentService.RemoveSystem(dataProcessingAgreement, systemId);

                if (result.Ok)
                    _repository.Update(dataProcessingAgreement);

                return result;
            });
        }

        private Result<DataProcessingAgreement, OperationError> UpdateProperties(int id, DataProcessingAgreementPropertyChanges changeSet)
        {
            if (changeSet == null)
                throw new ArgumentNullException(nameof(changeSet));

            return WithWriteAccess<DataProcessingAgreement>(id, dataProcessingAgreement =>
            {
                var updateNameError = UpdateName(dataProcessingAgreement, changeSet.NameChange);

                if (updateNameError.HasValue)
                    return updateNameError.Value;

                _repository.Update(dataProcessingAgreement);

                return dataProcessingAgreement;
            });
        }

        private Result<TSuccess, OperationError> WithWriteAccess<TSuccess>(int id, Func<DataProcessingAgreement, Result<TSuccess, OperationError>> mutation)
        {
            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var dataProcessingAgreement = result.Value;

            if (!_authorizationContext.AllowModify(dataProcessingAgreement))
                return new OperationError(OperationFailure.Forbidden);

            return mutation(dataProcessingAgreement);
        }

        private Maybe<OperationError> UpdateName(DataProcessingAgreement dataProcessingAgreement, Maybe<ChangedValue<string>> nameChange)
        {
            if (nameChange.IsNone)
                return Maybe<OperationError>.None;

            var newName = nameChange.Value.Value;

            return _namingService.ChangeName(dataProcessingAgreement, newName);
        }

        private Result<TSuccess, OperationError> WithReadAccess<TSuccess>(int id, Func<DataProcessingAgreement, Result<TSuccess, OperationError>> authorizedAction)
        {
            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var agreement = result.Value;

            if (!_authorizationContext.AllowReads(agreement))
                return new OperationError(OperationFailure.Forbidden);

            return authorizedAction(agreement);
        }
    }
}
