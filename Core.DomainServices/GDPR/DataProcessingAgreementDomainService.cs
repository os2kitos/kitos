using System;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingAgreementDomainService : IDataProcessingAgreementDomainService
    {
        private readonly IDataProcessingAgreementRepository _repository;

        public DataProcessingAgreementDomainService(IDataProcessingAgreementRepository repository)
        {
            _repository = repository;
        }

        public Maybe<OperationError> ValidateSuggestedNewAgreement(int organizationId, string name)
        {
            return ValidateNewName(organizationId, name, Maybe<int>.None);
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

        public Maybe<OperationError> ChangeName(DataProcessingAgreement dataProcessingAgreement, string newName)
        {
            if (dataProcessingAgreement == null) throw new ArgumentNullException(nameof(dataProcessingAgreement));

            var validation = ValidateNewName(dataProcessingAgreement.OrganizationId, newName, dataProcessingAgreement.Id);

            if (validation.Select(x => x.FailureType == OperationFailure.Conflict).GetValueOrDefault())
                return new OperationError(OperationFailure.Conflict);

            var result = dataProcessingAgreement.SetName(newName);
            if (result.HasValue)
                return result.Value;
            
            _repository.Update(dataProcessingAgreement);
            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> ValidateNewName(int organizationId, string name, Maybe<int> exceptId)
        {
            if (!DataProcessingAgreement.IsNameValid(name))
                return new OperationError("Name is invalid", OperationFailure.BadInput);

            if (ExistingDataProcessingAgreementWithSameNameInOrganization(organizationId, name, exceptId))
                return new OperationError("Existing DataProcessingAgreement", OperationFailure.Conflict);

            return Maybe<OperationError>.None;
        }

        private bool ExistingDataProcessingAgreementWithSameNameInOrganization(int organizationId, string name, Maybe<int> exceptId)
        {
            return _repository
                .Search(organizationId, name)
                .Transform
                (
                    inputQuery =>
                    exceptId
                        .Select(id => inputQuery.Where(x => x.Id != id))
                        .GetValueOrFallback(inputQuery)
                    )
                .Any();
            ;
        }
    }
}
