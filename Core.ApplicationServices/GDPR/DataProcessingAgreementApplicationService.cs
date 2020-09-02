using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.GDPR;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Shared;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.GDPR;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.GDPR
{
    public class DataProcessingAgreementApplicationService : IDataProcessingAgreementApplicationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IDataProcessingAgreementRepository _repository;
        private readonly IDataProcessingAgreementDomainService _domainService;

        public DataProcessingAgreementApplicationService(
            IAuthorizationContext authorizationContext,
            IDataProcessingAgreementRepository repository,
            IDataProcessingAgreementDomainService domainService)
        {
            _authorizationContext = authorizationContext;
            _repository = repository;
            _domainService = domainService;
        }

        public Result<DataProcessingAgreement, OperationError> Create(int organizationId, string name)
        {
            if (!_authorizationContext.AllowCreate<DataProcessingAgreement>(organizationId))
                return new OperationError(OperationFailure.Forbidden);

            return _domainService.Create(organizationId, name);
        }

        public Maybe<OperationError> ValidateSuggestedNewAgreement(int organizationId, string name)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            return _domainService.ValidateSuggestedNewAgreement(organizationId, name);
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
            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var agreement = result.Value;

            if (!_authorizationContext.AllowReads(agreement))
                return new OperationError(OperationFailure.Forbidden);

            return agreement;
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
            return UpdateWith(id, new DataProcessingAgreementPropertyChanges { NameChange = new ChangedValue<string>(name) });
        }

        private Result<DataProcessingAgreement, OperationError> UpdateWith(int id, DataProcessingAgreementPropertyChanges changeSet)
        {
            if (changeSet == null)
                throw new ArgumentNullException(nameof(changeSet));

            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var dataProcessingAgreement = result.Value;

            if (!_authorizationContext.AllowModify(dataProcessingAgreement))
                return new OperationError(OperationFailure.Forbidden);

            var updateNameError = UpdateName(dataProcessingAgreement, changeSet.NameChange);

            if (updateNameError.HasValue)
                return updateNameError.Value;

            _repository.Update(dataProcessingAgreement);

            return dataProcessingAgreement;
        }

        private Result<DataProcessingAgreement, OperationError> UpdateWith(int id, Func<DataProcessingAgreement, Result<DataProcessingAgreement, OperationError>> changes)
        {
            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var dataProcessingAgreement = result.Value;

            if (!_authorizationContext.AllowModify(dataProcessingAgreement))
                return new OperationError(OperationFailure.Forbidden);

            var changeResult = changes.Invoke(dataProcessingAgreement);
            
            if (changeResult.Failed)
                return changeResult.Error;

            _repository.Update(dataProcessingAgreement);

            return dataProcessingAgreement;
        }

        private Maybe<OperationError> UpdateName(DataProcessingAgreement dataProcessingAgreement, Maybe<ChangedValue<string>> nameChange)
        {
            if (nameChange.IsNone)
                return Maybe<OperationError>.None;

            var newName = nameChange.Value.Value;

            return _domainService.ChangeName(dataProcessingAgreement, newName);
        }
    }
}
