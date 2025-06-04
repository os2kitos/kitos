using System;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainServices.Repositories.GDPR;


namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationNamingService : IDataProcessingRegistrationNamingService
    {
        private readonly IDataProcessingRegistrationRepository _repository;

        public DataProcessingRegistrationNamingService(IDataProcessingRegistrationRepository repository)
        {
            _repository = repository;
        }

        public Maybe<OperationError> ValidateSuggestedNewRegistrationName(int organizationId, string name)
        {
            return ValidateNewName(organizationId, name, Maybe<int>.None);
        }

        public Maybe<OperationError> ChangeName(DataProcessingRegistration dataProcessingRegistration, string newName)
        {
            if (dataProcessingRegistration == null) throw new ArgumentNullException(nameof(dataProcessingRegistration));

            var validation = ValidateNewName(dataProcessingRegistration.OrganizationId, newName, dataProcessingRegistration.Id);

            if (validation.Select(x => x.FailureType == OperationFailure.Conflict).GetValueOrDefault())
                return new OperationError(OperationFailure.Conflict);

            var result = dataProcessingRegistration.SetName(newName);
            if (result.HasValue)
                return result.Value;
            
            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> ValidateNewName(int organizationId, string name, Maybe<int> exceptId)
        {
            if (!DataProcessingRegistration.IsNameValid(name))
                return new OperationError("Name is invalid", OperationFailure.BadInput);

            if (ExistingDataProcessingRegistrationWithSameNameInOrganization(organizationId, name, exceptId))
                return new OperationError("Existing DataProcessingRegistration", OperationFailure.Conflict);

            return Maybe<OperationError>.None;
        }

        private bool ExistingDataProcessingRegistrationWithSameNameInOrganization(int organizationId, string name, Maybe<int> exceptId)
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
        }
    }
}
