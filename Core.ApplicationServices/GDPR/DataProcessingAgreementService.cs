using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.GDPR;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.GDPR
{
    public class DataProcessingAgreementService : IDataProcessingAgreementService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IDataProcessingAgreementRepository _repository;

        public DataProcessingAgreementService(IAuthorizationContext authorizationContext, IDataProcessingAgreementRepository repository)
        {
            _authorizationContext = authorizationContext;
            _repository = repository;
        }

        public Result<DataProcessingAgreement, OperationError> Create(int organizationId, string name)
        {
            var error = ValidateSuggestedNewAgreement(organizationId, name);

            if (error.HasValue)
                return error.Value;

            var dataProcessingAgreement = new DataProcessingAgreement
            {
                OrganizationId = organizationId,
                Name = name
            };

            dataProcessingAgreement = _repository.Add(dataProcessingAgreement);

            return dataProcessingAgreement;
        }

        public Maybe<OperationError> ValidateSuggestedNewAgreement(int organizationId, string name)
        {
            if (!_authorizationContext.AllowCreate<DataProcessingAgreement>(organizationId))
                return new OperationError(OperationFailure.Forbidden);

            if (!DataProcessingAgreement.IsNameValid(name))
                return new OperationError("Name is invalid", OperationFailure.BadInput);

            if (ExistingDataProcessingAgreementWithSameNameInOrganization(organizationId, name))
                return new OperationError("Existing DataProcessingAgreement", OperationFailure.Conflict);

            return Maybe<OperationError>.None;
        }

        private bool ExistingDataProcessingAgreementWithSameNameInOrganization(int organizationId, string name)
        {
            return _repository.Search(organizationId, name).Any();
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
            
            if (take < 1 || skip < 0)
                return new OperationError("Invalid paging arguments", OperationFailure.BadInput);

            var dataProcessingAgreements = _repository
                .Search(organizationId, Maybe<string>.None)
                .OrderBy(x => x.Id)
                .Skip(skip)
                .Take(take);

            return Result<IQueryable<DataProcessingAgreement>, OperationError>.Success(dataProcessingAgreements);
        }

        public Result<DataProcessingAgreement, OperationError> UpdateProperty(int id, DataProcessingAgreementPropertyChanges changeSet)
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

        private Maybe<OperationError> UpdateName(DataProcessingAgreement dataProcessingAgreement, Maybe<ChangedValue<string>> nameChange)
        {
            if (nameChange.IsNone)
                return Maybe<OperationError>.None;

            var newName = nameChange.Value.Value;

            if (ExistingDataProcessingAgreementWithSameNameInOrganization(dataProcessingAgreement.OrganizationId, newName))
                return new OperationError(OperationFailure.Conflict);

            return dataProcessingAgreement.SetName(newName);
        }
    }
}
