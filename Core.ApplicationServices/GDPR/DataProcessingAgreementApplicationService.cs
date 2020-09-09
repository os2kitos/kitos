using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.GDPR;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Options;
using Core.ApplicationServices.Shared;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.GDPR;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.GDPR
{
    public class DataProcessingAgreementApplicationService : IDataProcessingAgreementApplicationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IDataProcessingAgreementRepository _repository;
        private readonly IDataProcessingAgreementNamingService _namingService;
        private readonly IOptionsService<DataProcessingAgreementRight, DataProcessingAgreementRole> _localRoleOptionsService;
        private readonly IDataProcessingAgreementRoleAssignmentService _roleAssignmentService;
        private readonly IUserRepository _userRepository;
        private readonly ITransactionManager _transactionManager;

        public DataProcessingAgreementApplicationService(
            IAuthorizationContext authorizationContext,
            IDataProcessingAgreementRepository repository,
            IDataProcessingAgreementNamingService namingService,
            IOptionsService<DataProcessingAgreementRight, DataProcessingAgreementRole> localRoleOptionsService,
            IDataProcessingAgreementRoleAssignmentService roleAssignmentService,
            IUserRepository userRepository,
            ITransactionManager transactionManager)
        {
            _authorizationContext = authorizationContext;
            _repository = repository;
            _namingService = namingService;
            _localRoleOptionsService = localRoleOptionsService;
            _roleAssignmentService = roleAssignmentService;
            _userRepository = userRepository;
            _transactionManager = transactionManager;
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

        public Result<IEnumerable<DataProcessingAgreementRole>, OperationError> GetAvailableRoles(int id)
        {
            return WithReadAccess(id, agreement =>
            {
                var dataProcessingAgreementRoles = _localRoleOptionsService.GetAvailableOptions(agreement.OrganizationId).ToList();
                return Result<IEnumerable<DataProcessingAgreementRole>, OperationError>.Success(dataProcessingAgreementRoles);
            });
        }

        public Result<IEnumerable<User>, OperationError> GetAvailableUsers(int id, int roleId, string nameEmailQuery, int pageSize)
        {
            return WithReadAccess(id, agreement =>
            {
                var availableRoles = GetAvailableRoles(id);

                if (availableRoles.Failed)
                    return availableRoles.Error;

                var targetRole = availableRoles.Value.FirstOrDefault(x => x.Id == roleId).FromNullable();

                if (targetRole.IsNone)
                    return new OperationError("Invalid role id", OperationFailure.BadInput);

                var candidates = _userRepository.SearchOrganizationUsers(agreement.OrganizationId, nameEmailQuery.FromNullable());

                candidates = _roleAssignmentService.GetUsersWhichCanBeAssignedToRole(agreement, targetRole.Value, candidates);

                return Result<IEnumerable<User>, OperationError>.Success(
                    candidates
                        .OrderBy(x => x.Id)
                        .Take(pageSize)
                        .ToList()
                );
            });
        }

        public Result<DataProcessingAgreementRight, OperationError> AssignRole(int id, int roleId, int userId)
        {
            return WithWriteAccess(id, agreement =>
            {
                using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
                var availableOption = _localRoleOptionsService.GetAvailableOption(agreement.OrganizationId, roleId);

                if (availableOption.IsNone)
                    return new OperationError("Selected role is not available in the organization", OperationFailure.BadInput);

                Maybe<User> userResult = _userRepository
                    .SearchOrganizationUsers(agreement.OrganizationId, Maybe<string>.None)
                    .FirstOrDefault(user => user.Id == userId);

                if (userResult.IsNone)
                    return new OperationError($"User Id {userId} is invalid in the context of organization with id '{agreement.OrganizationId}'", OperationFailure.BadInput);

                var assignmentResult = agreement.AssignRoleToUser(availableOption.Value, userResult.Value);

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
            //TODO: Check if insert is automatically applied - perhaps it is not and then we need to use the repo
            throw new NotImplementedException();
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
